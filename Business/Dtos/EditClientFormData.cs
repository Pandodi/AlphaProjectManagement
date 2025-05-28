using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Dtos;

public class EditClientFormData
{
    public string Id { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public string Email { get; set; } = null!;
}
