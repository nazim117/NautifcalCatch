using NauticalCatchChallenge.Models.Contracts;
using NauticalCatchChallenge.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticalCatchChallenge.Models
{
    public abstract class Diver : IDiver
    {
        private string name;
        private int oxygenLevel;
        private double competitionPoints;
        private bool hasHealthIssues;
        private List<string> catches;

        public Diver(string name, int oxygenLevel)
        {
            Name = name;
            OxygenLevel = oxygenLevel;
            hasHealthIssues = false;
            catches = new List<string>();
        }
        public string Name 
        { 
            get=>name;
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(ExceptionMessages.DiversNameNull);
                }
                name = value;
            }
        }

        public int OxygenLevel 
        { 
            get=> oxygenLevel; 
            protected set
            {
                oxygenLevel = Math.Max(0, value);
            }
        }

        public IReadOnlyCollection<string> Catch => catches.AsReadOnly();

        public double CompetitionPoints 
        { 
            get=>Math.Round(competitionPoints, 1); 
            private set => competitionPoints = value; 
        }

        public bool HasHealthIssues
        {
            get { return hasHealthIssues; }
            private set { hasHealthIssues = value; }
        }

        public void Hit(IFish fish)
        {
            OxygenLevel -= fish.TimeToCatch;
            catches.Add(fish.Name);
            CompetitionPoints += fish.Points;
        }

        public abstract void Miss(int timeToCatch);

        public abstract void RenewOxy();

        public void UpdateHealthStatus()
        {
            HasHealthIssues = !hasHealthIssues;
        }

        public override string ToString()
        {
            return $"Diver [ Name: {Name}, Oxygen left: {OxygenLevel}, Fish caught: {Catch.Count}, Points earned: {CompetitionPoints} ]";
        }
    }
}
