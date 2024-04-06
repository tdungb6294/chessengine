namespace Chess
{
    public class Player
    {
        public string username { get; set; }
        public PlayerColor playerColor { get; set; }
        public string contextId { get; set; }

        public Player(string username, PlayerColor playerColor, string contextId)
        {
            this.username = username;
            this.playerColor = playerColor;
            this.contextId = contextId;
        }
    }
}