namespace Domain.Models;

public class Project
{
    public string Id { get; set; } = null!;
    public string? Image { get; set; }
    public string ProjectName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Client Client { get; set; } = null!;
    public ICollection<User> Members { get; set; } = [];
    public Status? Status { get; set; }

    //With some help by chatGPT
    public string TimeLeft
    {
        get
        {
            var now = DateTime.Now;
            if (EndDate < now)
                return "Expired";

            var diff = EndDate - now;

            if (diff.TotalDays < 1)
                return $"{diff.Hours}h {diff.Minutes}m left";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} day{(diff.TotalDays >= 2 ? "s" : "")} left";
            else if (diff.TotalDays < 30)
                return $"{(int)(diff.TotalDays / 7)} week{(diff.TotalDays / 7 >= 2 ? "s" : "")} left";
            else
                return $"{(int)(diff.TotalDays / 30)} month{(diff.TotalDays / 30 >= 2 ? "s" : "")} left";
        }
    }

}