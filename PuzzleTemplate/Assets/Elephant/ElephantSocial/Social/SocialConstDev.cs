using ElephantSDK;

namespace ElephantSocial
{
    public class SocialConstDev
    {
        //Social
        private static string Api { get; }
        private static string PlayerApi { get; }
        public static string PlayerEp { get; }
        public static string PlayerUp { get; }
        public static string GetPlayerWithSocialIDEp { get; }
        
        //Leaderboard
        private static string LeaderboardEp { get; }
        public static string PlayerScoreEp { get; }
        public static string BoardEp { get; }
        public static string EventGroupEp { get; }
        public static string UserEp { get; }
        
        //Tournaments
        private static string TournamentEp { get; }
        public static string TournamentsAll { get; }
        public static string TournamentsMine { get; }
        public static string TournamentsResult { get; }
        public static string TournamentAddScoreEp { get; }
        public static string TournamentAddScoresEp { get; }
        public static string TournamentInit { get; }
        public static string TournamentGetBoardEp { get; }
        public static string TournamentJoinEp { get; }
        public static string TournamentClaimEp { get; }
        public static string TournamentRefreshEp { get; }
        public static string TournamentGetStatesEp { get; }
        
        // Tournament Matches
        public static string TournamentAddMatchEp { get; }
        public static string TournamentListMatchesEp { get; }
        
        //HonorWall
        public static string HonorWallEp { get; }
        public static string HonorWallGrantEp { get; }
        
        //HealthCheck
        public static string HealthCheckEp { get; }
        
        //Teams
        private static string TeamEp { get; }
        public static string GetTeamsEp { get; }
        public static string SearchTeamsEp { get; }
        public static string GetTeamDetailEp { get; }
        public static string JoinTeamEp { get; }
        public static string CreateTeamEp { get; }
        public static string UpdateTeamEp { get; }
        public static string LeaveTeamEp { get; }
        public static string PromoteTeamMemberEp { get; }
        public static string DemoteTeamMemberEp { get; }
        public static string KickTeamMemberEp { get; }
        public static string AcceptJoinEp { get; }
        public static string RejectJoinEp { get; }
        public static string JoinRequestsEp { get; }
        public static string PlayerJoinRequestsEp { get; }
        public static string ChatEp { get; }
        public static string IncrementStatEp { get; }
        public static string GetInboxEp { get; }
        public static string InboxReadEp { get; }
        
        static SocialConstDev()
        {
            Api = "https://game-server-proxy-dev.rollic.gs/proxy/api/v1";
            var healthcheckEp = "https://game-server-proxy-dev.rollic.gs";

            PlayerApi = Api + "/player";
            PlayerEp = PlayerApi + "";
            PlayerUp = PlayerApi + "/save";
            GetPlayerWithSocialIDEp = PlayerApi + "/{socialId}/detail";
            
            LeaderboardEp = Api + "/leaderboard";
            PlayerScoreEp = LeaderboardEp + "/{id}/score";
            BoardEp = LeaderboardEp + "/get_board_ext";
            EventGroupEp = LeaderboardEp + "/get_group";
            UserEp = LeaderboardEp + "/get_user";
            
            TournamentEp = Api + "/tournament";
            TournamentsAll = TournamentEp + "/all";
            TournamentsMine = TournamentEp + "/mine";
            TournamentsResult = TournamentEp + "/result";
            TournamentAddScoreEp = TournamentEp + "/score";
            TournamentAddScoresEp = TournamentEp + "/scores";
            TournamentInit = TournamentEp + "/init";
            TournamentGetBoardEp = TournamentEp + "/get_board";
            TournamentJoinEp = TournamentEp + "/join";
            TournamentClaimEp = TournamentEp + "/result/claim";
            TournamentRefreshEp = TournamentEp + "/refresh";
            TournamentGetStatesEp = TournamentEp + "/states";
            
            // Tournament Matches
            TournamentAddMatchEp = TournamentEp + "/add_match";
            TournamentListMatchesEp = TournamentEp + "/list_matches";
            
            HonorWallEp = Api + "/honorwall";
            HonorWallGrantEp = HonorWallEp + "/grant";
            
            TeamEp = Api + "/team";
            GetTeamsEp = TeamEp + "/suggest";
            SearchTeamsEp = TeamEp + "/search";
            GetTeamDetailEp = TeamEp + "/detail";
            JoinTeamEp = TeamEp + "/join";
            CreateTeamEp = TeamEp + "/create";
            UpdateTeamEp = TeamEp + "/update";
            LeaveTeamEp = TeamEp + "/leave";
            PromoteTeamMemberEp = TeamEp + "/promote_member";
            DemoteTeamMemberEp = TeamEp + "/demote_member";
            KickTeamMemberEp = TeamEp + "/kick_member";
            AcceptJoinEp = TeamEp + "/accept";
            RejectJoinEp = TeamEp + "/reject";
            JoinRequestsEp = TeamEp + "/join_requests";
            PlayerJoinRequestsEp = TeamEp + "/player_join_requests";
            IncrementStatEp = TeamEp + "/stat/increment";
            GetInboxEp = Api + "/inbox/get";
            InboxReadEp = Api + "/inbox/mark_as_read";
            
            HealthCheckEp = healthcheckEp;
            
            ChatEp = "wss://chat-server-dev.rollic.gs/chat_server/api/v1";
        }
    }
}