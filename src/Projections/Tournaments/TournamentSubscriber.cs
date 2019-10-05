using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Tournament.Domain.Abstractions;
using Orleans.Tournament.Domain.Abstractions.Grains;
using Orleans.Tournament.Domain.Tournaments;
using Orleans.Tournament.Projections.Teams;
using Constants = Orleans.Tournament.Domain.Helpers;

namespace Orleans.Tournament.Projections.Tournaments
{
    [ImplicitStreamSubscription(Constants.StreamNamespace)]
    public class TournamentSubscriber : SubscriberGrain
    {
        private readonly ITeamQueryHandler _teamQueryHandler;
        private readonly ProjectionManager<TournamentProjection> _projectionManager;

        public TournamentSubscriber(
            PostgresOptions postgresOptions,
            ITeamQueryHandler teamQueryHandler,
            ILogger<TournamentSubscriber> logger)
            : base(
                new StreamOptions(Constants.TournamentStream, Constants.StreamNamespace),
                new PrefixLogger(logger, "[Tournament][Projection]"))
        {
            _projectionManager = new ProjectionManager<TournamentProjection>("read", "tournament_projection", postgresOptions);
            _teamQueryHandler = teamQueryHandler;
        }

        public override async Task HandleAsync(object evt, StreamSequenceToken token = null)
        {
            switch (evt)
            {
                case TournamentCreated obj:
                    await Handle(obj);
                    break;
                case TeamAdded obj:
                    await Handle(obj);
                    break;
                case TournamentStarted obj:
                    await Handle(obj);
                    break;
                case MatchResultSet obj:
                    await Handle(obj);
                    break;
                default:
                    PrefixLogger.LogError(
                        "unhandled event of type [{evtType}] for resource id: [{grainId}]", evt.GetType().Name, this.GetPrimaryKey());
                    break;
            }
        }

        private async Task Handle(TournamentCreated evt)
        {
            var projection = TournamentProjection.New.SetName(evt.TournamentId, evt.Name);
            await _projectionManager.UpdateProjection(this.GetPrimaryKey(), projection);
        }

        private async Task Handle(TeamAdded evt)
        {
            var team = await _teamQueryHandler.GetTeamAsync(evt.TeamId);
            var teamObj = new TournamentProjection.Team(team.Id, team.Name);

            var projection = await _projectionManager.GetProjectionAsync(this.GetPrimaryKey());
            await _projectionManager.UpdateProjection(this.GetPrimaryKey(), projection.AddTeam(teamObj));
        }

        // The idea is to serve the data in a friendly way to the user, in this case
        // I am using the same value object that the state is using but it can be changed here
        private async Task Handle(TournamentStarted evt)
        {
            var projection = await _projectionManager.GetProjectionAsync(this.GetPrimaryKey());

            await _projectionManager.UpdateProjection(
                this.GetPrimaryKey(),
                projection.AddFixture(Fixture.Create(evt.Teams)));
        }

        private async Task Handle(MatchResultSet evt)
        {
            var projection = await _projectionManager.GetProjectionAsync(this.GetPrimaryKey());
            projection.Fixture.SetMatchResult(evt.MatchInfo);
            await _projectionManager.UpdateProjection(this.GetPrimaryKey(), projection);
        }
    }
}
