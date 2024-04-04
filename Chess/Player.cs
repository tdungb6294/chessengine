namespace Chess
{
    public class Player : IEquatable<Player>
    {
        public string username { get; set; }
        public PlayerColor playerColor { get; set; }

        public Player(string username, PlayerColor playerColor)
        {
            this.username = username;
            this.playerColor = playerColor;
        }

        public bool Equals(Player? other)
        {
            if (other is null) return false;
            if (other.username == this.username) return true;
            return true;
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