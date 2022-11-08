using API.Configs;
using API.Models;
using AutoMapper;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Services
{
    public class AuthService
    {
        private readonly DataContext _context;
        private readonly AuthConfig _config;

        public AuthService(DataContext context, IOptions<AuthConfig> config)
        {
            _context = context;
            _config = config.Value;
        }

        private async Task<UserSession> GetSessionByRefreshToken(Guid refreshTokenID)
        {
            var sesh = await _context.Sessions.Include(u => u.UserOfThisSession).FirstOrDefaultAsync(s => s.RefreshTokenId == refreshTokenID);
            if (sesh == null)
            {
                throw new Exception("No session with such refresh token id found");
            }
            return sesh;
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.SymmetricSecurityKey()
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var token);

            if (token is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshTokenID").Value is string refreshTokenIdString &&
                Guid.TryParse(refreshTokenIdString, out var refreshTokenID))
            {
                var session = await GetSessionByRefreshToken(refreshTokenID);
                if (!session.IsActive)
                {
                    throw new Exception("session is non-active");
                }
                var user = session.UserOfThisSession;

                session.RefreshTokenId = Guid.NewGuid();
                await _context.SaveChangesAsync();
                return GenerateToken(user, session);
            }
            throw new Exception("Invalid user");
        }

        private TokenModel GenerateToken(User user, UserSession session)
        {
            if (session.UserOfThisSession == null)
                throw new Exception("Invalid session, has no user");

            var dtNow = DateTime.Now;
            var accessJWT = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                claims: new Claim[]
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                new Claim("userID", user.Id.ToString()),
                new Claim("sessionID", session.Id.ToString()),
            },
                notBefore: dtNow,
                expires: dtNow.AddMinutes(_config.Lifetime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedAccess = new JwtSecurityTokenHandler().WriteToken(accessJWT);

            var refreshJWT = new JwtSecurityToken(
                issuer: _config.Issuer,
                claims: new Claim[]
            {
                new Claim("refreshTokenID", session.RefreshTokenId.ToString()),
            },
                notBefore: dtNow,
                expires: dtNow.AddHours(_config.Lifetime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refreshJWT);

            return new TokenModel(encodedAccess, encodedRefresh);
        }

        private async Task<User> GetUserByCredentials(string login, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == login.ToLower());
            if (user == null)
            {
                throw new Exception("No user by this login found");
            }

            if (!HashHelper.CompareHash(password, user.PasswordHashed))
            {
                throw new Exception("Invalid password");
            }
            return user;
        }

        public async Task<UserSession> GetSessionByID(Guid sessionID)
        {
            var sesh = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == sessionID);
            if (sesh == null)
            {
                throw new Exception("No session by this id found");
            }
            return sesh;
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredentials(login, password);
            var session = await _context.Sessions.AddAsync(new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RefreshTokenId = Guid.NewGuid(),
                UserOfThisSession = user,
                SessionCreatedTime = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return GenerateToken(user, session.Entity);

        }
    }
}
