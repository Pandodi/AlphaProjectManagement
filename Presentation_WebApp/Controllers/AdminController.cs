using Business.Dtos;
using Business.Services;
using Domain.Extensions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation_WebApp.Models;
using Presentation_WebApp.ViewModels;

namespace Presentation_WebApp.Controllers;

[Authorize]
public class AdminController(IClientService clientService, IMemberService memberService, IUserService userService) : Controller
{
    private readonly IClientService _clientService = clientService;
    private readonly IMemberService _memberService = memberService;
    private readonly IUserService _userService = userService;

    /* -------------------- Members page --------------------*/

    [Authorize(Roles = "Admin")]
    [Route("/admin/members")]
    [HttpGet]
    public async Task<IActionResult> Members()
    {
        var result = await _userService.GetUsersAsync();
        var model = new MembersViewModel();

        if (result.Succeeded)
        {
            model.Users = result.Result!;
        }
        else
        {
            ViewBag.Error = result.Error;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(AddMemberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        string? userImage = null;
        if (model.MemberImage != null && model.MemberImage.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.MemberImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("MemberImage", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            if (model.MemberImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("MemberImage", "File size must not exceed 5MB.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await model.MemberImage.CopyToAsync(stream);
            }

            userImage = $"/uploads/{fileName}";
        }

        var addMemberFormData = model.MapTo<AddMemberFormData>();
        addMemberFormData.UserImage = userImage;
        addMemberFormData.Address = new MemberAddress
        {
            StreetName = model.Address.StreetName!,
            PostalCode = model.Address.PostalCode!,
            City = model.Address.City!
        };

        var result = await _memberService.CreateMemberAsync(addMemberFormData);

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return StatusCode(result.StatusCode, new { success = false, message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> EditMember(EditMemberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        string? userImage = null;
        if (model.MemberImage != null && model.MemberImage.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(model.MemberImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("MemberImage", "Only JPG, JPEG, PNG, and GIF files are allowed.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            if (model.MemberImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("MemberImage", "File size must not exceed 5MB.");
                return BadRequest(new { success = false, errors = ModelState });
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads"));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await model.MemberImage.CopyToAsync(stream);
            }

            userImage = $"/uploads/{fileName}";
        }

        var editMemberFormData = model.MapTo<EditMemberFormData>();
        editMemberFormData.UserImage = userImage;
        editMemberFormData.Address = new MemberAddress
        {
            StreetName = model?.Address.StreetName!,
            PostalCode = model?.Address.PostalCode!,
            City = model?.Address.City!
        };

        var result = await _memberService.UpdateMemberFromUserAsync(editMemberFormData);

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return StatusCode(result.StatusCode, new { success = false, message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMember(string id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (result.Succeeded)
            return Ok(new { success = true });

        return StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }


    /* -------------------- Clients page --------------------*/

    [Authorize(Roles = "Admin")]
    [Route("/admin/clients")]
    [HttpGet]
    public async Task<IActionResult> Clients()
    {
        var result = await _clientService.GetClientsAsync();
        var model = new ClientsViewModel();

        if (result.Succeeded)
        {
            model.Clients = result.Result!;
        } else
        {
            ViewBag.Error = result.Error;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddClient(AddClientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x=> x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors});
        }

        var addClientFormData = model.MapTo<AddClientFormData>();
        var result = await _clientService.CreateClientAsync(addClientFormData);

        if(result.Succeeded)
        {
            return Ok(new {success = true});
        }

        return StatusCode(result.StatusCode, new { success = false, message = result.Error });
    }


    [HttpPost]
    public async Task<IActionResult> EditClient(EditClientViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.Errors.Select(x => x.ErrorMessage).ToArray());
            return BadRequest(new { success = false, errors });
        }

        var editClientFormData = model.MapTo<EditClientFormData>();
        var result = await _clientService.UpdateClientAsync(editClientFormData);

        if (result.Succeeded)
        {
            return Ok(new { success = true });
        }

        return StatusCode(result.StatusCode, new { success = false, message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteClient(string id)
    {
        var result = await _clientService.DeleteClientAsync(id);
        if (result.Succeeded)
            return Ok(new { success = true });

        return StatusCode(result.StatusCode, new { success = false, error = result.Error });
    }

    [AllowAnonymous]
    [Route("denied")]
    public IActionResult Denied()
    {
        return View();
    }
}
