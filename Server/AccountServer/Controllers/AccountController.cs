using AccountServer.DB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
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

                //TODO: 서버 리스트를 가져오는 로직을 넣어주세요. 
                res.ServerList = new List<ServerInfo>()
                {
                    new ServerInfo() { ServerName = "NitServer1", ServerIp = "127.0.0.1", CrowdedLevel = 0},
                    new ServerInfo() { ServerName = "NitServer2", ServerIp = "127.0.0.1", CrowdedLevel = 3}
                };
            }

            return res;
        }
    }
}
