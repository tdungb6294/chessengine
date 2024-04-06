namespace Chess
{
    public class Player : IEquatable<Player>
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

        public bool Equals(Player? other)
        {
            if (other is null) return false;
            if (other.contextId == this.contextId) return true;
            return false;
        }

        public static bool operator ==(Player left, Player right)
        {
            if (left is null)
                return right is null;

            return left.Equals(right);
        }

        public static bool operator !=(Player left, Player right)
        {
            return !(left == right);
        }
    }
}