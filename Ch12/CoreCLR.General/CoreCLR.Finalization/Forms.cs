using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CoreCLR.Finalization
{
    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-43
    public class Application
    {
        public void Run()
        {
            List<WeakReference> observer = new List<WeakReference>();

            MainWindow mainWindow = new MainWindow();
            while (true)
            {
                Thread.Sleep(1000);
                ChildWindow childWindow = new ChildWindow(mainWindow);
                observer.Add(new WeakReference(childWindow));

                childWindow.RegisterEvents(mainWindow); // Leave uncommented to leak child windows
                childWindow.Show();

                GC.Collect();
                foreach (var weakReference in observer)
                {
                    Console.Write(weakReference.IsAlive ? "1" : "0");
                }
            }
        }
    }

    public class WeakEventListener
    {
        private readonly WeakReference weakTarget;

        public static void Subsribe(object source, string eventName, object target, Action<string> eventHandler)
        {
            Delegate del = eventHandler;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // Listing 12-42
    public class MainWindow
    {
        public delegate void SettingsChangedEventHandler(string message);

        public event SettingsChangedEventHandler SettingsChanged;
    }

    public class ChildWindow
    {
        private MainWindow parent;

        public ChildWindow(MainWindow parent)
        {
            this.parent = parent;
        }

        public void RegisterEvents(MainWindow parent)
        {
            // ChildWindow - target
            // MainWindow - source
            parent.SettingsChanged += OnParentSettingsChanged;
            //WeakEventListener.Subsribe(parent, "SettingsChanged", this, OnParentSettingsChanged);
            //WeakEventManager<MainWindow, string>.AddHandler(parent, "SettingsChanged", OnParentSettingsChanged);
        }

        private void OnParentSettingsChanged(string message)
        {
            Console.WriteLine(message);
        }

        public void Show()
        {
            Console.WriteLine("ChildWindows showed");
        }
    }
}
