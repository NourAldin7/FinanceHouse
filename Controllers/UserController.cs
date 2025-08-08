using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly MongoDbService _service;

    public UsersController(MongoDbService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _service.GetAllUsersAsync();
        return Ok(new
        {
            status = true,
            message = "User found.",
            data = users
        });

    }

    [HttpGet("by-phone")]
    public async Task<IActionResult> GetUserByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new
            {
                status = false,
                message = "Phone number is required.",
                data = (object)null
            });
        }

        var user = await _service.GetUserByPhoneAsync(phone);

        if (user == null)
        {
            return NotFound(new
            {
                status = false,
                message = "User not found.",
                data = (object)null
            });
        }

        return Ok(new
        {
            status = true,
            message = "User found.",
            data = user
        });
    }


    [HttpGet("search")]
    public async Task<IActionResult> SearchUserByPhone([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(new
            {
                status = false,
                message = "Phone number is required.",
                data = (object)null
            });
        }

        var user = await _service.SearchBenfAsync(phone);

        if (user == null)
        {
            return NotFound(new
            {
                status = false,
                message = "User not found.",
                data = (object)null
            });
        }

        return Ok(new
        {
            status = true,
            message = "User found.",
            data = user
        });
    }

    [HttpPost("add-beneficiary")]
    public async Task<IActionResult> AddBeneficiary([FromBody] AddBeneficiaryRequest request)
    {
        var user = await _service.GetUserByPhoneAsync(request.userPhone);
        var beneficiaryUser = await _service.GetUserByPhoneAsync(request.beneficiaryPhone);

        if (user == null || beneficiaryUser == null)
        {
            return BadRequest(new
            {
                status = false,
                message = "User or Beneficiary not found"
            });
        }

        if (user.beneficiaries == null)
            user.beneficiaries = new List<Beneficiary>();

        if (user.beneficiaries.Count() >= 5)
        {
            return BadRequest(new
            {
                status = false,
                message = "User already has 5 active beneficiaries"
            });
        }

        // Create the beneficiary object
        var newBeneficiary = new Beneficiary
        {
            _id = beneficiaryUser._id,
            name = beneficiaryUser.name,
            nickname = request.nickname,
            limit = 3000,
        };

        // Add to user and update in DB
        user.beneficiaries.Add(newBeneficiary);
        await _service.UpdateUserAsync(user);

        return Ok(new
        {
            status = true,
            message = "Beneficiary added successfully.",
            data = newBeneficiary
        });
    }

    [HttpPost("delete-beneficiary")]
    public async Task<IActionResult> DeleteBeneficiary(DeleteBeneficiaryRequest dto)
    {
        var result = await _service.DeleteUserAsync(dto.userId, dto.beneficiaryId);

        if (result)
        {
            return Ok(new { status = true, message = "Beneficiary removed successfully" });
        }
        return NotFound(new { status = false, message = "User or Beneficiary not found" });
    }

    [HttpPost("topup")]
    public async Task<IActionResult> TopupBeneficiary(TopupRequest dto)
    {
        var result = await _service.TopUpAsync(dto);

        if (result)
        {
            return Ok(new { status = true, message = "Beneficiary removed successfully" });
        }
        return NotFound(new { status = false, message = "User or Beneficiary not found" });
    }
}
