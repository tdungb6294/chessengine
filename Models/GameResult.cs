public class GameResult
{
    public int Id { get; set; }
    public string PlayerName1 { get; set; } = "";
    public string PlayerName2 { get; set; } = "";
    public string GameStatus { get; set; } = "";
    public string GameBoard { get; set; } = "";
    public DateTime CreatedOn { get; set; }
}