
using Business.Dtos;
using Business.Models;
using Data.Entities;
using Data.Models;
using Data.Repositories;
using Domain.Extensions;
using Domain.Models;

namespace Business.Services;

public interface IClientService
{
    Task<ClientResult> CreateClientAsync(AddClientFormData formData);
    Task<ClientResult> DeleteClientAsync(string clientId);
    Task<ClientResult> GetClientsAsync();
    Task<ClientResult> UpdateClientAsync(EditClientFormData formData);
}

public class ClientService(IClientRepository clientRepository) : IClientService
{
    private readonly IClientRepository _clientRepository = clientRepository;

    public async Task<ClientResult> GetClientsAsync()
    {
        var result = await _clientRepository.GetAllAsync();
        return result.MapTo<ClientResult>();
    }

    public async Task<ClientResult> CreateClientAsync(AddClientFormData formData)
    {
        if (formData == null)
            return new ClientResult { Succeeded = false, StatusCode = 400, Error = "Not all required fields are supplied." };

        var clientEntity = formData.MapTo<ClientEntity>();
        var result = await _clientRepository.AddAsync(clientEntity);

        if (result.Succeeded)
        {
            var client = clientEntity.MapTo<Client>();
            return new ClientResult { Succeeded = true, StatusCode = 200, Result = [client] };
        };

        return new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ClientResult> UpdateClientAsync(EditClientFormData formData)
    {
        if (formData == null)
            return new ClientResult { Succeeded = false, StatusCode = 400, Error = "Not all required field are supplied." };

        var clientEntity = formData.MapTo<ClientEntity>();
        var result = await _clientRepository.UpdateAsync(clientEntity);

        return result.Succeeded
            ? new ClientResult { Succeeded = true, StatusCode = 200 }
            : new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

    public async Task<ClientResult> DeleteClientAsync(string clientId)
    {
        if (clientId == null)
            return new ClientResult { Succeeded = false, StatusCode = 400, Error = "Can't find client" };

        var clientEntity = new ClientEntity { Id = clientId };
        var result = await _clientRepository.DeleteAsync(clientEntity);

        return result.Succeeded
            ? new ClientResult { Succeeded = true, StatusCode = 200 }
            : new ClientResult { Succeeded = false, StatusCode = result.StatusCode, Error = result.Error };
    }

}
