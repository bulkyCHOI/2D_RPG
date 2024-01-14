using System;
using System.Xml;

namespace PacketGenerator
{
    class Program
    {
        static string genPacket;
        static ushort packetId;
        static string packetEnums;

        static string clientRegister; //클라이언트에서 서버로 전송하는 용도의 패킷 ex) packeName = C_ㅇㅇㅇ
        static string serverRegister; //서버에서 서버로 전송하는 용도의 패킷       ex) packeName = S_ㅇㅇㅇ

        static void Main(string[] args)
        {
            string pdlPath = "../../../PDL.xml"; 

            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace= true,
            };

            if(args.Length > 0 )    //시작할때 파라미터가 뭔가 들어왓다면
                pdlPath= args[0];

            using (XmlReader r = XmlReader.Create(pdlPath, settings)) //using을 사용하면 사용후에 Dispose를 알아서 해준다.
            {
                r.MoveToContent();

                while (r.Read())
                {
                    if (r.Depth == 1 && r.NodeType == XmlNodeType.Element)
                        ParsePacket(r);
                    //Console.WriteLine(r.Name + " " + r["name"]);
                }
                string fileText = string.Format(PacketFormat.fileFormat, packetEnums, genPacket);
                File.WriteAllText("GenPackets.cs", fileText);
                string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);
                string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }
        }
        public static void ParsePacket(XmlReader r)
        {
            if (r.NodeType == XmlNodeType.EndElement)   //</> endElement일 경우
                return;
            if (r.Name.ToLower() != "packet")   // packet 태그가 아니라면
            {
                Console.WriteLine("Invalid packet node");
                return;
            }
            string packetName = r["name"];
            if(string.IsNullOrEmpty(packetName))
            {
                Console.WriteLine("Packet without name");
                return;
            }

            Tuple<string, string, string> t = ParseMembers(r);
            genPacket += string.Format(PacketFormat.packetFormat, packetName, t.Item1, t.Item2, t.Item3); //매개변수 4개를 넣고 코드를 string으로 만들어준다.
            
            //엔터+탭정렬
            if (string.IsNullOrEmpty(packetEnums) == false)
                packetEnums += Environment.NewLine + "\t";
            packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId);

            //엔터+탭정렬
            if (string.IsNullOrEmpty(packetEnums) == false)
                packetEnums += Environment.NewLine;

            if(packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                clientRegister += string.Format(PacketFormat.managerRegisterFormat, packetName);
            else
                serverRegister += string.Format(PacketFormat.managerRegisterFormat, packetName);
        }

        //{1} 멤버 변수들
        //{2} 멤버 변수 Read
        //{3} 멤버 변수 Write
        public static Tuple<string, string, string> ParseMembers(XmlReader r)
        {
            string packetName = r["name"];

            string memberCode = "";
            string readCode = "";
            string writeCode = "";

            int depth = r.Depth + 1;    //depth를 벗어나면 종료시키기 위해
            while (r.Read())
            {
                if (r.Depth != depth)
                    break;
                string memberName = r["name"];
                if (string.IsNullOrEmpty(memberName))
                {
                    Console.WriteLine("Member without name");
                    return null;
                }

                //line by line으로 엔터 쳐주기 위해서
                if (string.IsNullOrEmpty(memberCode)==false)
                    memberCode += Environment.NewLine;
                if (string.IsNullOrEmpty(readCode) ==false)
                    readCode += Environment.NewLine;
                if (string.IsNullOrEmpty(writeCode) ==false)
                    writeCode += Environment.NewLine;

                string memberType = r.Name.ToLower();
                switch (memberType)
                {
                    case "byte":
                    case "sbyte":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                        writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                        break;
                    case "bool":
                    case "short":
                    case "ushort":
                    case "int":
                    case "long":
                    case "float":
                    case "double":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                        writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                        break;
                    case "string":
                        memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                        readCode += string.Format(PacketFormat.readStringFormat, memberName);
                        writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                        break;
                    case "list":
                        Tuple<string, string, string> t = ParseList(r);
                        memberCode += t.Item1;
                        readCode += t.Item2;
                        writeCode += t.Item3;
                        break;
                    default:
                        break;
                }
            }

            //탭 맞춰주기
            memberCode = memberCode.Replace("\n", "\n\t");
            readCode = readCode.Replace("\n", "\n\t\t");
            writeCode = writeCode.Replace("\n", "\n\t\t");

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static Tuple<string, string, string> ParseList(XmlReader r)
        {
            string listName = r["name"];
            if(string.IsNullOrEmpty(listName))
            {
                Console.WriteLine("List without name");
                return null;
            }

            Tuple<string, string, string> t = ParseMembers(r);

            string memberCode = string.Format(PacketFormat.memberListFormat, 
                FirstCharToUpper(listName), 
                FirstCharToLower(listName), 
                t.Item1, 
                t.Item2, 
                t.Item3);
            string readCode = string.Format(PacketFormat.readListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));
            string writeCode = string.Format(PacketFormat.writeListFormat,
                FirstCharToUpper(listName),
                FirstCharToLower(listName));

            return new Tuple<string, string, string>(memberCode, readCode, writeCode);
        }

        public static string ToMemberType(string memberType)    //To~ 함수명을 만들어주는 함수
        {
            switch (memberType)
            {
                case "bool":
                    return "ToBoolean";
                case "short":
                    return "ToInt16";
                case "ushort":
                    return "ToUInt16";
                case "int":
                    return "ToInt32";
                case "long":
                    return "ToInt64";
                case "float":
                    return "ToSingle";
                case "double":
                    return "ToDouble";
                default:
                    return "";
            }
        }

        public static string FirstCharToUpper(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToUpper()+input.Substring(1);
        }
        public static string FirstCharToLower(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            return input[0].ToString().ToLower() + input.Substring(1);
        }
    }
}