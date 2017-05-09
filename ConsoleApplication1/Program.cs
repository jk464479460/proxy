using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var baby = LoggingProxy.Wrap(new Baby());
            Console.WriteLine(baby.Name);
            try
            {
                baby.Name = "jdjd";
                baby.Sleep("bei jing");
            }catch(Exception ex)
            {
                Console.WriteLine("Exception {0}", ex.Message);
            }
        }
    }

  
    class BTAttribute : Attribute
    {

    }
    class Baby : MarshalByRefObject
    {
        private string _name;
        public string Name { get { return "Annabelle"; } set { _name = value; } }

        [BT]
        public void Sleep(string a)
        {
            Console.WriteLine("Teething in progress");
        }
    }

    public class LoggingProxy : RealProxy
    {
        readonly object target;

        LoggingProxy(object target):base(target.GetType())
        {
            this.target = target;
        }
        public static T Wrap<T>(T target) where T : MarshalByRefObject
        {
            return (T) new LoggingProxy(target).GetTransparentProxy();
        }
        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;

            var execInject = false;
            foreach (var item in methodCall.MethodBase.GetCustomAttributes(false))
            {
                var name = item.GetType().Name;
                if ("BTAttribute".Equals(name))
                {
                    execInject = true;

                }
            }
            if (methodCall != null && execInject)
            {
                return HandleMethodCall(methodCall);
            }
            var ret=methodCall.MethodBase.Invoke(target,null);
            return new ReturnMessage(ret,null,0,methodCall.LogicalCallContext,methodCall);
        }

        IMessage HandleMethodCall(IMethodCallMessage methodCall)
        {
           
            //if (execInject == true)
            {
                Console.WriteLine("Calling Method {0}...{1}", methodCall.MethodName, methodCall.GetArg(0));
                try
                {
                    var result = methodCall.MethodBase.Invoke(target, methodCall.InArgs);
                    return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Calling {0}", methodCall.MethodName, methodCall.GetArg(0));
                    return new ReturnMessage(ex, methodCall);
                }
            }
            //return null;
        }


    }
}
