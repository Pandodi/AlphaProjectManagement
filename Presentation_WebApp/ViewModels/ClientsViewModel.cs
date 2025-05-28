using Domain.Models;

namespace Presentation_WebApp.Models;

public class ClientsViewModel
{
    public IEnumerable<Client> Clients { get; set; } = [];
    public AddClientViewModel AddClientForm { get; set; } = new AddClientViewModel();
    public EditClientViewModel EditClientForm { get; set; } = new EditClientViewModel();
}
