using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _uow;

        public UsersController(IMapper mapper, IPhotoService photoService, IUnitOfWork uow)
        {
            _mapper = mapper;
            _photoService = photoService;
            _uow = uow;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            // return Ok(await _uow.UserRepository.GetUsersAsync());

            // var users = await _uow.UserRepository.GetUsersAsync();

            // var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);

            var gender = await _uow.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUsername = User.GetUsername();

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }

            var users = await _uow.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            // var user = await _uow.UserRepository.GetUserByUserNameAsync(username);

            // return _mapper.Map<MemberDto>(user);

            return await _uow.UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // var username = User.Identity.Name;

            var username = User.GetUsername();
            var user = await _uow.UserRepository.GetUserByUserNameAsync(username);

            if (user == null) return NotFound();

            _mapper.Map(memberUpdateDto, user);
            _uow.UserRepository.Update(user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var results = await _photoService.AddPhotoAsync(file);

            if (results.Error != null) return BadRequest(results.Error.Message);

            var photo = new Photo
            {
                Url = results.SecureUrl.AbsoluteUri,
                PublicId = results.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _uow.Complete())
            {
                return CreatedAtAction(nameof(GetUser), 
                    new { username = user.UserName }, 
                    _mapper.Map<PhotoDto>(photo));
            }
                
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(p => p.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _uow.UserRepository.GetUserByUserNameAsync(User.GetUsername());

            if (user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var results = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (results.Error != null) return BadRequest(results.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _uow.Complete()) return Ok();

            return BadRequest("Failed to delete photo");
        }
    }
}
