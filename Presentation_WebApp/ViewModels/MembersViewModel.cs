using Domain.Models;
using Presentation_WebApp.ViewModels;

namespace Presentation_WebApp.Models;

public class MembersViewModel
{
    public IEnumerable<User> Users { get; set; } = [];
    public AddMemberViewModel AddMemberForm { get; set; } = new AddMemberViewModel();
    public EditMemberViewModel EditMemberForm { get; set; } = new EditMemberViewModel();
}
