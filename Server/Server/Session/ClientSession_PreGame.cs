using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
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
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            Console.WriteLine($"UniqueId({loginPacket.UniqueId})");

            // TODO: 이런저런 보안 체크
            if(PlayerServerState.ServerStateLogin != ServerState)
                return;

            LobbyPlayers.Clear();

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
                    //AccountDbId 메모리에 기억
                    AccountDbId = account.AccountDbId;

                    S_Login sLogin = new S_Login() { LoginOk = 1 };
                    foreach(PlayerDb playerDB in account.Players)
                    {
                        LobbyPlayerInfo info = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDB.PlayerDbId,
                            Name = playerDB.PlayerName,
                            StatInfo = new StatInfo()
                            {
                                Level = playerDB.level,
                                MaxHp = playerDB.maxHp,
                                MaxMp = playerDB.maxMp,
                                Hp = playerDB.hp,
                                Mp = playerDB.mp,
                                Speed = playerDB.speed,
                                Attack = playerDB.attack,
                                Defence = playerDB.defence,
                                TotalExp = playerDB.totalExp
                            }
                        };
                        //메모리에도 들고 있어야 한다. >> 나중에 Client가 EnterGame할때 사용
                        LobbyPlayers.Add(info);

                        //패킷에 넣어준다.
                        sLogin.Players.Add(info);
                    }
                    Send(sLogin);

                    //로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    // 계정이 없으면 생성
                    AccountDb newAccount = new AccountDb() { AccountName = loginPacket.UniqueId };
                    db.Accounts.Add(newAccount);
                    bool success = db.SaveChangesEx();
                    if(success == false)
                        return;

                    //AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;

                    S_Login sLogin = new S_Login() { LoginOk = 1 };
                    Send(sLogin);

                    //로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if(playerInfo == null)
                return;

            MyPlayer = ObjectManager.Instance.Add<Player>();    // 플레이어 생성
            {
                MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
                MyPlayer.Info.Name = playerInfo.Name;
                MyPlayer.Info.PosInfo.State = CreatureState.Idle;
                MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
                MyPlayer.Info.PosInfo.PosX = 0;
                MyPlayer.Info.PosInfo.PosY = 0;
                MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
                MyPlayer.Session = this;

                S_ItemList itemListPacket = new S_ItemList();

                //아이템 목록을 갖고 온다.
                using (AppDbContext db = new AppDbContext())
                {
                    List<ItemDb> items = db.Items
                        .Where(i => i.OwnerDbId == MyPlayer.PlayerDbId).ToList();

                    foreach (ItemDb itemDb in items)
                    {
                        Item item = Item.MakeItem(itemDb);
                        if (item != null)
                        {
                            MyPlayer.Inventory.AddItem(item);   //메모리에 넣어준다.
                            ItemInfo info = new ItemInfo();
                            info.MergeFrom(item.Info);
                            itemListPacket.Items.Add(info); //클라이언트에게 보낼 패킷을 만들고
                        }
                    }
                }
                Send(itemListPacket);   //클라이언트에게 보낸다.
            }

            ServerState = PlayerServerState.ServerStateGame;

            GameLogic.Instance.Push(() =>
            { 
                GameRoom room = GameLogic.Instance.Find(1);
                room.Push(room.EnterGame, MyPlayer, true);	// 방에 플레이어 입장	//Job 방식으로 변경
            });	

        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            // TODO : 이런저런 보안 체크
            if (ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb playerDb = db.Players
                    .Where(p => p.PlayerName == createPacket.Name).FirstOrDefault();
                
                if(playerDb != null)
                {
                    // 이미 존재하는 플레이어 >> null로 보내준다.
                    Send(new S_CreatePlayer() { Player = null });
                }
                else
                {
                    //1레벨 스탯정보 추출
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);

                    //DB에 플레이어 정보 생성
                    PlayerDb player = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        level = stat.Level,
                        maxHp = stat.MaxHp,
                        maxMp = stat.MaxMp,
                        hp = stat.MaxHp,
                        mp = stat.MaxMp,
                        speed = stat.Speed,
                        attack = stat.Attack,
                        defence = stat.Defence,
                        totalExp = 0,
                        AccountDbId = AccountDbId
                    };

                    db.Players.Add(player);
                    bool success = db.SaveChangesEx();
                    if (success == false)
                        return;

                    //메모리에도 들고 있다.
                    LobbyPlayerInfo info = new LobbyPlayerInfo()
                    {
                        PlayerDbId = player.PlayerDbId,
                        Name = player.PlayerName,
                        StatInfo = new StatInfo()
                        {
                            Level = player.level,
                            MaxHp = player.maxHp,
                            MaxMp = player.maxMp,
                            Hp = player.hp,
                            Mp = player.mp,
                            Speed = player.speed,
                            Attack = player.attack,
                            Defence = player.defence,
                            TotalExp = player.totalExp
                        }
                    };
                    LobbyPlayers.Add(info);

                    //클라이언트에 전송
                    Send(new S_CreatePlayer() { Player = info });

                }
            }
        }

    }
}
