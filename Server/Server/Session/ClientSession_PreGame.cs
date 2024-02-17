using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public partial class ClientSession: PacketSession
    {
        public void HandleLogin(C_Login loginPacket)
        {
            Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

            // TODO: 이런저런 보안 체크
            if(PlayerServerState.ServerStateLogin != ServerState)
                return;

            // DB에서 유저 정보 체크
            // TODO: 문제가 있긴 있다.
            // 서버를 만들때는 크래시와 해킹을 항상고려해야한다.
            // - 동시에 다른 사람이 같은 uniqueId로 로그인하는 경우
            // - 동일한 패킷을 여러번 보내는 경우
            // - 패킷을 쌩뚱맞은 타이밍에 그냥 보내는 경우
            using (AppDbContext db = new AppDbContext())
            {
                AccountDb account = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId).FirstOrDefault();

                if (account != null)
                {
                    S_Login sLogin = new S_Login() { LoginOk = 1 };
                    Send(sLogin);
                }
                else
                {
                    // 계정이 없으면 생성
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    db.SaveChanges();   // TODO : 예외처리

                    S_Login sLogin = new S_Login() { LoginOk = 1 };
                    Send(sLogin);
                }
            }
        }
    }
}
