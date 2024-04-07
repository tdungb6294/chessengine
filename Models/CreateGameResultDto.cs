using System.ComponentModel.DataAnnotations;

public class CreateGameResultDto
{
    [Required(ErrorMessage = "PlayerName1 is required")]
    [StringLength(50, ErrorMessage = "PlayerName1 cannot exceed 255 characters")]
    public string PlayerName1 { get; set; } = "";

    [Required(ErrorMessage = "PlayerName2 is required")]
    [StringLength(50, ErrorMessage = "PlayerName2 cannot exceed 255 characters")]
    public string PlayerName2 { get; set; } = "";

    [Required(ErrorMessage = "GameStatus is required")]
    [StringLength(50, ErrorMessage = "GameStatus cannot exceed 255 characters")]
    public string GameStatus { get; set; } = "";
}
