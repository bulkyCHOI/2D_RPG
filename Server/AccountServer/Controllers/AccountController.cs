using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;
        SharedDbContext _sharedContext;

        public AccountController(AppDbContext context, SharedDbContext sharedContext)
        {
            _context = context;
            _sharedContext = sharedContext;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPakcetRes CreateAccount([FromBody] CreateAccountPakcetReq req)  // CreateAccountPakcetReq를 받아서 CreateAccountPakcetRes를 반환 JSON으로 반환
        {
            CreateAccountPakcetRes res = new CreateAccountPakcetRes();

            AccountDb account = _context.Accounts.AsNoTracking()
                .Where(p => p.AccountName == req.AccountName).FirstOrDefault();

            if(account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    AccountName = req.AccountName,
                    Password = req.Password
                });

                bool success = _context.SaveChangesEx();
                res.CreateOk = success;
            }
            else
            {
                res.CreateOk = false;
            }

            return res;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPakcetRes LoginAccount([FromBody] LoginAccountPakcetReq req)
        {
            LoginAccountPakcetRes res = new LoginAccountPakcetRes();

            AccountDb account = _context.Accounts.AsNoTracking()
                .Where(p => p.AccountName == req.AccountName && p.Password == req.Password).FirstOrDefault();

            if(account == null)
            {
                res.LoginOk = false;
            }
            else
            {
                res.LoginOk = true;

                // 토큰발급
                DateTime expired = DateTime.UtcNow;
                expired.AddSeconds(600);

                TokenDb tokenDb = _sharedContext.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                if(tokenDb != null)
                {
                    tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                    tokenDb.Expired = expired;
                    _sharedContext.SaveChangesEx();
                }
                else
                {
                    tokenDb = new TokenDb()
                    {
                        AccountDbId = account.AccountDbId,
                        Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                        Expired = expired
                    };
                    _sharedContext.Add(tokenDb);
                    _sharedContext.SaveChangesEx();
                }

                res.AccountId = account.AccountDbId;
                res.Token = tokenDb.Token;
                res.ServerList = new List<ServerInfo>();

                foreach (var server in _sharedContext.ServerInfos)
                {
                    res.ServerList.Add(new ServerInfo()
                    {
                        ServerName = server.ServerName,
                        ServerIp = server.ServerIP,
                        ServerPort = server.ServerPort,
                        BusyScore = server.BusyCcore
                    });
                }
            }

            return res;
        }
    }
}
