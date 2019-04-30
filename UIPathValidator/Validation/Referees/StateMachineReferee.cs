using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class StateMachineReferee : IWorkflowReferee
    {
        public string Code => "state-machine";

        public Workflow Workflow { get; protected set; }
        public XamlReader Reader { get; protected set; }

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            Workflow = workflow;
            Reader = workflow.GetXamlReader();
            var smTags = Reader.Document.Descendants(XName.Get("StateMachine", Reader.Namespaces.DefaultNamespace));
            
            foreach (var smTag in smTags)
            {
                if (smTag.IsInsideCommentOut(Reader.Namespaces))
                    continue;

                var stateMachine = new StateMachine(smTag, this);
                stateMachine.Parse();
                results.AddRange(stateMachine.Validate());
            }

            return results;
        }
    }

    internal class StateMachine
    {
        XElement Element;
        public StateMachineReferee Referee { get; protected set; }
        public XmlNamespaceManager Namespaces { get; protected set; }

        Dictionary<string, State> States;

        State StartState;

        public StateMachine(XElement element, StateMachineReferee referee)
        {
            Referee = referee;
            Namespaces = Referee.Reader.Namespaces;
            Element = element;

            States = new Dictionary<string, State>();
        }

        public void Parse()
        {
            var stateTags = Element.Descendants(XName.Get("State", Namespaces.DefaultNamespace));

            foreach (var stateTag in stateTags)
            {
                var state = new State(stateTag, this);
                States.Add(state.ReferenceID, state);
            }

            foreach (var item in States)
                item.Value.ParseTransitions();

            var initStateValue = Element.Attribute("InitialState")?.Value ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(initStateValue))
            {
                var initStateRef = State.ExtractReference(initStateValue);
                if (States.ContainsKey(initStateRef))
                {
                    StartState = States[initStateRef];
                }
            }

            if (StartState == null && States.Count > 0)
                StartState = States.First().Value;
        }

        public State GetState(string reference)
        {
            if (States.ContainsKey(reference))
                return States[reference];
            return null;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            var results = new List<ValidationResult>();

            StartState.Paint();
            IEnumerable<State> items;
            do
            {
                items =
                    from item in States.Values
                    where item.PaintBlack()
                    select item;
            }
            while (items.Count() > 0);

            var orphanStates =
                from item in States.Values
                where item.Color == GraphColor.White
                select item;

            foreach (var item in orphanStates)
                results.Add(new StateMachineValidationResult(Referee.Workflow, item.DisplayName, ValidationResultType.Warning, "The state is never reached. Either use it or delete it."));

            var deadEndStates =
                from item in States.Values
                where item.Color == GraphColor.Gray
                select item;

            foreach (var item in deadEndStates)
                results.Add(new StateMachineValidationResult(Referee.Workflow, item.DisplayName, ValidationResultType.Error, "The state never reaches a final state."));

            return results;
        }
    }

    internal class State
    {
        private XElement Element { get; set; }

        private StateMachine Machine { get; set; }

        ICollection<State> Exits { get; set; }
        public int ExitsCount => Exits.Count;

        public bool IsFinal { get; protected set; }

        public string ReferenceID { get; protected set; }

        public string DisplayName { get; protected set; }

        public GraphColor Color { get; protected set; }

        public State(XElement element, StateMachine machine)
        {
            this.Element = element;
            this.Machine = machine;
            Color = GraphColor.White;

            Exits = new List<State>();

            ReferenceID = Element.Attribute(XName.Get("Name", Machine.Namespaces.LookupNamespace("x")))?.Value ?? DateTime.Now.Ticks.ToString();
            DisplayName = Element.Attribute("DisplayName")?.Value ?? "State";
            string isFinalValue = Element.Attribute("IsFinal")?.Value ?? string.Empty;
            IsFinal = isFinalValue.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase);
        }

        public void ParseTransitions()
        {
            var stateTransitionTag = Element.Element(XName.Get("State.Transitions", Machine.Namespaces.DefaultNamespace));
            if (stateTransitionTag == null)
                return;

            var transitionTags = stateTransitionTag.Elements(XName.Get("Transition", Machine.Namespaces.DefaultNamespace));
            foreach (var transitionTag in transitionTags)
            {
                var toValue = transitionTag.Attribute("To")?.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(toValue))
                {
                    var transitionToTag = transitionTag.Element(XName.Get("Transition.To", Machine.Namespaces.DefaultNamespace));
                    if (transitionToTag == null)
                        continue;

                    var childState = transitionToTag.Element(XName.Get("State", Machine.Namespaces.DefaultNamespace));
                    var childName = childState.Attribute(XName.Get("Name", Machine.Namespaces.LookupNamespace("x"))).Value;
                    Exits.Add(Machine.GetState(childName));
                }
                else
                {
                    Exits.Add(Machine.GetState(ExtractReference(toValue)));
                }
            }
        }

        public void Paint()
        {
            if (Color == GraphColor.White)
            {
                if (IsFinal)
                {
                    Color = GraphColor.Black;
                }
                else
                {
                    Color = GraphColor.Gray;
                    foreach (var item in Exits)
                    {
                        item.Paint();
                        if (item.Color == GraphColor.Black)
                            Color = GraphColor.Black;
                    }
                }
            }
        }

        public bool PaintBlack()
        {
            if (Color == GraphColor.Gray)
            {
                if (Exits.Any(x => x.Color == GraphColor.Black))
                {
                    Color = GraphColor.Black;
                    return true;
                }
            }
            return false;
        }

        #region Static

        private static Regex referenceRegex = new Regex(@"x\:Reference ([._0-9a-zA-Z]+)");

        public static string ExtractReference(string attributeString)
        {
            if (string.IsNullOrWhiteSpace(attributeString))
                return string.Empty;

            var match = referenceRegex.Match(attributeString);
            if (!match.Success)
                return string.Empty;

            return match.Groups[1].Value;
        }

        #endregion Static
    }
}