using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
    黑科技：协议派发新方法
       对delegate进行模板封装
 */


namespace delegate_wrapper
{
    public class BaseProtocol
    {
        public string sth;
    }
    public class BaseProtocolInvokeCall
    {
        public virtual void Invoke(BaseProtocol p) { }
    }
    public class TProtocolInvokeCall<T> : BaseProtocolInvokeCall where T : BaseProtocol
    {
        System.Action<T> Callback;
        public override void Invoke(BaseProtocol p) 
        {
            if (Callback != null)
            {
                Callback(p as T);
            }
        }
        public void Register(System.Action<T> callback)
        {
            Callback = callback;
        }
    }
    static class ProtocolDict
    {
        static Dictionary<int, Type> Dict;
        public static void Register(int id, Type t)
        {
            if(Dict == null)
            {
                Dict = new Dictionary<int,Type>();
            }
            if(Dict.ContainsKey(id))
            {
                Dict[id] = t;
            }
            else
            {
                Dict.Add(id, t);
            }
        }
        public static Type GetProtocolType(int id)
        {
            if (Dict != null && Dict.ContainsKey(id))
                return Dict[id];
            else
                return null;
        }
    }

    public class ProtocolDispatcher
    {
        Dictionary<Type, BaseProtocolInvokeCall> Dict = new Dictionary<Type, BaseProtocolInvokeCall>();

        public void Register<T>(System.Action<T> callback) where T : BaseProtocol
        {
            Type id = typeof(T);
            var invokeobj = new TProtocolInvokeCall<T>();
            invokeobj.Register(callback);
            if (Dict.ContainsKey(id))
                Dict[id] = invokeobj;
            else
                Dict.Add(id, invokeobj);
        }
        public void SimulateDispatch(BaseProtocol b)
        {
            Type pid = b.GetType();
            if (Dict.ContainsKey(pid))
            {
                Dict[pid].Invoke(b);
            }
        }

        // 模拟从网络端读取协议
        public void SimulateReadDataFromNet()
        {
            const int MAX = 5;
            Random r = new Random();

            int cnt = 10;
            while (--cnt > 0)
            {
                // 模拟读取协议id
                int id = r.Next() % MAX;
                Type t = ProtocolDict.GetProtocolType(id);
                if (t != null)
                {
                    BaseProtocol p = Activator.CreateInstance(t) as BaseProtocol;
                    if (p != null)
                    {
                        // 模拟读取数据并设置内容
                        p.sth = "123123123";
                        // 然后在模拟派发收到的协议
                        SimulateDispatch(p);
                    }
                }
                else
                {
                    Console.WriteLine("无效的协议号：" + id);
                }
            }

        }
    }


    public class Protocol1 : BaseProtocol
    { 
    }
    public class Protocol2 : BaseProtocol
    {
    }


    class Test
    { 
        ProtocolDispatcher dispatcher = new ProtocolDispatcher();
        
        public Test()
        {
            // 注册协议
            ProtocolDict.Register(1, typeof(Protocol1));
            ProtocolDict.Register(2, typeof(Protocol2));

            // 注册协议处理函数
            dispatcher.Register<Protocol1>(OnProtocol1);
            dispatcher.Register<Protocol2>(OnProtocol2);
        }
        public void DoTest()
        {
            //dispatcher.SimulateDispatch(new Protocol1());
            dispatcher.SimulateReadDataFromNet();
        }

        void OnProtocol1(Protocol1 p)
        {
            Console.WriteLine("process p:" + p.GetType().Name);
        }
        void OnProtocol2(Protocol2 p)
        {
            Console.WriteLine("2 process p:" + p.GetType().Name);
        }
    }





    class Program
    {
        static void Main(string[] args)
        {
            new Test().DoTest();

            Console.WriteLine("按任意键继续...");
            Console.ReadKey();
        }

    }
}
