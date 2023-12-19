using NauticalCatchChallenge.Core.Contracts;
using NauticalCatchChallenge.Models;
using NauticalCatchChallenge.Models.Contracts;
using NauticalCatchChallenge.Repositories;
using NauticalCatchChallenge.Utilities.Messages;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NauticalCatchChallenge.Core
{
    public class Controller : IController
    {
        private DiverRepository diverRepository;
        private FishRepository fishRepository;

        public Controller()
        {
            diverRepository = new DiverRepository();
            fishRepository = new FishRepository();
        }

        public string ChaseFish(string diverName, string fishName, bool isLucky)
        {
            if (!diverRepository.Models.Any(existingDiver => existingDiver.Name == diverName))
            {
                return string.Format(OutputMessages.DiverNotFound, diverRepository.GetType().Name, diverName);
            }

            if (!fishRepository.Models.Any(existingDiver => existingDiver.Name == fishName))
            {
                return string.Format(OutputMessages.FishNotAllowed, fishName);
            }

            IDiver diver = diverRepository.Models.FirstOrDefault(d => d.Name == diverName);
            IFish fish = fishRepository.Models.FirstOrDefault(d => d.Name == fishName);

            if (diver.HasHealthIssues)
            {
                return string.Format(OutputMessages.DiverHealthCheck, diverName);
            }

            if (diver.OxygenLevel < fish.TimeToCatch)
            {
                diver.Miss(fish.TimeToCatch);

                if (diver.OxygenLevel == 0)
                {
                    diver.UpdateHealthStatus();
                }

                return string.Format(OutputMessages.DiverMisses, diverName, fishName);
            }

            if (diver.OxygenLevel == fish.TimeToCatch)
            {
                if (isLucky)
                {
                    diver.Hit(fish);

                    if (diver.OxygenLevel == 0)
                    {
                        diver.UpdateHealthStatus();
                    }

                    return string.Format(OutputMessages.DiverHitsFish, diverName, fish.Points, fishName);
                }
                else
                {
                    diver.Miss(fish.TimeToCatch);

                    if (diver.OxygenLevel == 0)
                    {
                        diver.UpdateHealthStatus();
                    }

                    return string.Format(OutputMessages.DiverMisses, diverName, fishName);
                }
            }

            if (diver.OxygenLevel > fish.TimeToCatch)
            {
                diver.Hit(fish);
                if (diver.OxygenLevel == 0)
                {
                    diver.UpdateHealthStatus();
                }
            }

            return string.Format(OutputMessages.DiverHitsFish, diverName, fish.Points, fishName);
        }

        public string CompetitionStatistics()
        {
            var diversInCompetition = diverRepository.Models
                .OrderByDescending(d=>d.CompetitionPoints)
                .ThenBy(d=>d.Catch.Count)
                .ThenBy(d=>d.Name)
                .ToList();

            var healthyDivers = diversInCompetition.Where(d => d.HasHealthIssues == false).ToList();

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("**Nautical-Catch-Challenge**");

            foreach (var diver in healthyDivers)
            {
                stringBuilder.AppendLine(diver.ToString());
            }

            return stringBuilder.ToString().TrimEnd();
        }

        public string DiveIntoCompetition(string diverType, string diverName)
        {
            if (diverRepository.Models.Any(existingDiver => existingDiver.Name == diverName))
            {
                return string.Format(OutputMessages.DiverNameDuplication, diverName, diverRepository.GetType().Name);
            }

            IDiver diver;
            if (diverType == nameof(FreeDiver)) 
            {
                diver = new FreeDiver(diverName);
            }
            else if (diverType == nameof(ScubaDiver))
            {
                diver = new ScubaDiver(diverName);
            }
            else
            {
                return string.Format(OutputMessages.DiverTypeNotPresented, diverType);
            }

            diverRepository.AddModel(diver);
            return string.Format(OutputMessages.DiverRegistered, diverName, diverRepository.GetType().Name);
        }

        public string DiverCatchReport(string diverName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            IDiver diver = diverRepository.Models.FirstOrDefault(d => d.Name == diverName);

            stringBuilder.AppendLine(diver.ToString());
            stringBuilder.AppendLine("Catch Report:");

            foreach (var caugthFishName in diver.Catch)
            {
                IFish fish = fishRepository.Models.FirstOrDefault(d => d.Name == caugthFishName);
                stringBuilder.AppendLine(fish.ToString());
            }

            return stringBuilder.ToString().TrimEnd();
        }

        public string HealthRecovery()
        {
            List<IDiver> unhealthyDivers = diverRepository.Models.Where(d=>d.HasHealthIssues == true).ToList();

            int healedDiversCount = 0;

            foreach (var diver in unhealthyDivers)
            {
                diver.UpdateHealthStatus();
                diver.RenewOxy();
                healedDiversCount++;
            }

            return string.Format(OutputMessages.DiversRecovered, healedDiversCount);
        }

        public string SwimIntoCompetition(string fishType, string fishName, double points)
        {
            if (fishRepository.Models.Any(existingDiver => existingDiver.Name == fishName))
            {
                return string.Format(OutputMessages.FishNameDuplication, fishName, fishRepository.GetType().Name);
            }

            IFish fish;
            if (fishType == nameof(DeepSeaFish))
            {
                fish = new DeepSeaFish(fishName, points);
            }
            else if (fishType == nameof(ReefFish))
            {
                fish = new ReefFish(fishName, points);
            }
            else if(fishType == nameof(PredatoryFish))
            {
                fish = new PredatoryFish(fishName, points);
            }
            else
            {
                return string.Format(OutputMessages.FishTypeNotPresented, fishType);
            }

            fishRepository.AddModel(fish);
            return string.Format(OutputMessages.FishCreated, fishName);
        }
    }
}
