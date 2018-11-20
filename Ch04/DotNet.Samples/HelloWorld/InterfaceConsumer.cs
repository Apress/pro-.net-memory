namespace HelloWorld
{
	public class Consumer 
	{
		public void CallAll()
		{
			IInterface0 iface0 = new Class0();
			IInterface1 iface1 = new Class1();
			IInterface2 iface2 = new Class2();
			IInterface3 iface3 = new Class3();
			IInterface4 iface4 = new Class4();
			IInterface5 iface5 = new Class5();
			iface0.Method();
			iface1.Method();
			iface2.Method();
			iface3.Method();
			iface4.Method();
			iface5.Method();
		}
	}
}