using ElephantUniTask.Threading.Tasks;
using ElephantSDK;
using ElephantSocial.Core;
using ElephantSocial.Model;
using ElephantSocial.Team.Model.Enum;

namespace ElephantSocial.Team
{
    public class TeamMember
    {
        public string id;
        public string name;
        public int score;
        public int helps;
        public TeamMemberRole role;
        public int badge;
        public string profilePicture;

        public static TeamMember FromServerModel(Model.TeamMemberResponse serverModel)
        {
            return new TeamMember
            {
                id = serverModel.SocialId,
                name = serverModel.PlayerInfo.PlayerName,
                score = serverModel.Score,
                helps = serverModel.Helps,
                role = (TeamMemberRole)serverModel.Role,
                badge = serverModel.PlayerInfo.Badge,
                profilePicture = serverModel.PlayerInfo.ProfilePicture
            };
        }

        public Player GetProfile()
        {
            return new Player
            {
                playerName = name,
                socialId = id,
                badge = badge,
                profilePicture = profilePicture,
                status = 1
            };
        }
        
        public async UniTask<bool> PromoteMemberAsync()
        {
            try
            {
                await TeamService.PromoteMemberAsync(id);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("TeamMember", $"Failed to promote member: {ex.Message}");
                return false;
            }
        }

        public async UniTask<bool> DemoteMemberAsync()
        {
            try
            {
                await TeamService.DemoteMemberAsync(id);
                return true;
            }
            catch (TeamOperationException ex)
            {
                ElephantLog.LogError("TeamMember", $"Failed to demote member: {ex.Message}");
                return false;
            }
        }
    }
}