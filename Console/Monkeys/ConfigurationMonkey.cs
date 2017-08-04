using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Octopus.Client;
using Octopus.Client.Model;
using SeaMonkey.ProbabilitySets;
using Serilog;
using System;

namespace SeaMonkey.Monkeys
{
    public class ConfigurationMonkey : Monkey
    {
        public ConfigurationMonkey(OctopusRepository repository) : base(repository)
        {
        }

        public void CreateRecords(int numberOfSubscriptions, int numberOfTeams, int numberOfUsers)
        {
            CreateDisabledSubscriptions(numberOfSubscriptions);
            CreateEmptyTeams(numberOfTeams);
            CreateInactiveUsers(numberOfUsers);
        }

        #region Subscriptions

        public void CreateDisabledSubscriptions(int numberOfRecords)
        {
            var currentCount = Repository.Subscriptions.FindAll().Count();
            for (var x = currentCount; x <= numberOfRecords; x++)
                CreateDisabledSubscription(x);
        }

        private SubscriptionResource CreateDisabledSubscription(int prefix)
        {
            return
                Repository.Subscriptions.Create(new SubscriptionResource()
                {
                    Name = "Subscription-" + prefix.ToString("000"),
                    Type = SubscriptionType.Event,
                    IsDisabled = true, // We're just load testing some data, this can be disabled.
                    EventNotificationSubscription = new EventNotificationSubscription
                    {
                        Filter = new EventNotificationSubscriptionFilter
                        {

                        },
                        EmailTeams = new ReferenceCollection(new string[] { "teams-administrators" }),
                        EmailFrequencyPeriod = TimeSpan.Parse("99.00:00:00"),
                        EmailShowDatesInTimeZoneId = "E. Australia Standard Time",
                    }
                });
        }

        #endregion

        #region Teams

        public void CreateEmptyTeams(int numberOfRecords)
        {
            var currentCount = Repository.Teams.FindAll().Count();
            for (var x = currentCount; x <= numberOfRecords; x++)
                CreateEmptyTeam(x);
        }

        private TeamResource CreateEmptyTeam(int prefix)
        {
            return
                Repository.Teams.Create(new TeamResource()
                {
                    Name = "Team-" + prefix.ToString("000"),
                });
        }

        #endregion

        #region Users

        public void CreateInactiveUsers(int numberOfRecords)
        {
            var users = Repository.Users.FindAll();
            var currentCount = Repository.Users.FindAll().Count();
            for (var x = currentCount; x <= numberOfRecords; x++)
                CreateInactiveUser(x);
        }

        private UserResource CreateInactiveUser(int prefix)
        {
            return
                Repository.Users.Create(new UserResource()
                {
                    Username = "User-" + prefix.ToString("000"),
                    DisplayName = "User-" + prefix.ToString("000"),
                    IsActive = false,
                    IsService = false,
                    EmailAddress = "rick-" + prefix.ToString("000") + "@morty.com",
                    Password = "RickAndMortyForPresident"
                });
    }

        #endregion
    }
}
