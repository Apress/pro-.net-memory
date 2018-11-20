using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DOD;
using ECS;
using ObjectLayoutInspector;
using Node = OOP.Node;

namespace CoreCLR.DataOrientedDesign
{
    class Program
    {
        static void Main(string[] args)
        {
            //TypeLayout.PrintLayout<OOP.Customer>();
            //TypeLayout.PrintLayout<DOD.CustomerValue>();

            OOP.Node root = new Node(3);
            root.AddChild(new Node(4));
            OOP.Node right = new Node(5);
            right.AddChild(new Node(6));
            root.AddChild(right);
            root.Process();

            DOD.Tree tree = new Tree(root);
            tree.Process();

            ECS.Manager manager = new Manager();
            manager.RegisterSystem(new MoveSystem());
            manager.RegisterSystem(new RenderingSystem());

            ECS.Entity entity = manager.CreateEntity();
            var startPosition = new PositionComponent() {X = 0.0, Y = 0.0};
            var initialMovement = new MovableComponent() {Direction = 0.0, Speed = 1.0};
            ComponentManager<PositionComponent>.Register(in entity, in startPosition);
            ComponentManager<MovableComponent>.Register(in entity, in initialMovement);

            manager.Update();
            manager.Update();
        }
    }
}

namespace OOP
{

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-46
    class Customer
    {
        public double Earnings;
        // ... some other fields ...
        public DateTime DateOfBirth;
        // ... some other fields ...
        public bool IsSmoking;
        // ... some other fields ...
        public double Scoring;
        // ... some other fields ...
        public HealthData Health;
        public AuxiliaryData Auxiliary;

        public void UpdateScoring()
        {
            this.Scoring = this.Earnings * (this.IsSmoking ? 0.8 : 1.0 * ProcessAge(this.DateOfBirth));
        }

        private double ProcessAge(DateTime dateOfBirth) => 1.0;
    }

    public class AuxiliaryData
    {
    }

    public class HealthData
    {
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-49
    class CustomerRepository
    {
        List<Customer> customers = new List<Customer>();

        public void UpdateScorings()
        {
            foreach (var customer in customers)
            {
                customer.UpdateScoring();
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-51
    public class Node
    {
        public int Value { get; set; }
        public List<Node> Children = new List<Node>();
        public Node(int value) => Value = value;
        public void AddChild(Node child) => Children.Add(child);

        public void Process()
        {
            InternalProcess(null);
        }

        private void InternalProcess(Node parent)
        {
            if (parent != null)
                this.Value += parent.Value;
            foreach (var child in Children)
            {
                child.InternalProcess(this);
            }
        }
    }
}

namespace ECS
{
    ///////////////////////////////////////////////////////////////////////
    // Listing 14-53
    public readonly struct Entity
    {
        public readonly long Id;

        public Entity(long id)
        {
            Id = id;
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-54
    public struct PositionComponent
    {
        public double X;
        public double Y;
    }

    public struct MovableComponent
    {
        public double Speed;
        public double Direction;
    }

    public struct LivingComponent
    {
        public double Fatigue;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-55
    public class ComponentManager<T> where T : unmanaged
    {
        private static T Nothing = default;
        private static int registeredComponentsCount = 0;
        private static T[] registeredComponents = ArrayPool<T>.Shared.Rent(128);
        private static Dictionary<long, int> entityIdtoComponentIndex = new Dictionary<long, int>();

        public static void Register(in Entity entity, in T initialValue)
        {
            registeredComponents[registeredComponentsCount] = initialValue;
            entityIdtoComponentIndex.Add(entity.Id, registeredComponentsCount);
            registeredComponentsCount++;
        }

        public static ref T TryGetRegistered(in Entity entity, out bool result)
        {
            if (entityIdtoComponentIndex.TryGetValue(entity.Id, out int index))
            {
                result = true;
                return ref registeredComponents[index];
            }
            result = false;
            return ref Nothing;
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-56
    public abstract class SystemBase
    {
        public abstract void Update(List<Entity> entities);
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-57
    public class Manager
    {
        private List<Entity> entities = new List<Entity>();
        private List<SystemBase> systems = new List<SystemBase>();

        public void RegisterSystem(SystemBase system)
        {
            systems.Add(system);
        }
        public Entity CreateEntity()
        {
            var entity = new Entity(entities.Count);
            entities.Add(entity); // Boxed but that's ok
            return entity;
        }

        public void Update()
        {
            foreach (var system in systems)
            {
                system.Update(entities);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-58
    public class MoveSystem : SystemBase
    {
        public override void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                bool hasPosition = false;
                bool isMovable = false;
                ref var position = ref ComponentManager<PositionComponent>.TryGetRegistered(in entity, out hasPosition);
                ref var movable = ref ComponentManager<MovableComponent>.TryGetRegistered(in entity, out isMovable);
                if (hasPosition && isMovable)
                {
                    position.X += movable.Speed;
                }
            }
        }
    }

    public class RenderingSystem : SystemBase
    {
        public override void Update(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                bool isPosition = false;
                ref var position = ref ComponentManager<PositionComponent>.TryGetRegistered(in entity, out isPosition);
                if (isPosition)
                {
                    Console.WriteLine($"{position.X}, {position.Y}");
                }
            }
        }
    }
}

namespace DOD
{
    ///////////////////////////////////////////////////////////////////////
    // Listing 14-52
    public class Tree
    {
        private ValueNode[] _valueNodes;

        public Tree(OOP.Node root)
        {
            PrecalculateNode(root);
        }

        private void PrecalculateNode(OOP.Node node)
        {
            _valueNodes = new ValueNode[4];
            _valueNodes[0].Value = 3;
            _valueNodes[0].Parent = -1;
            _valueNodes[1].Value = 4;
            _valueNodes[1].Parent = 0;
            _valueNodes[2].Value = 5;
            _valueNodes[2].Parent = 0;
            _valueNodes[3].Value = 6;
            _valueNodes[3].Parent = 2;
        }

        public void Process()
        {
            for (int i = 1; i < _valueNodes.Length; ++i)
            {
                ref var node = ref _valueNodes[i];
                node.Value = node.Value + _valueNodes[node.Parent].Value;
            }
        }
    }

    public struct ValueNode
    {
        public int Value;
        public int Parent;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-50
    class CustomerRepository
    {
        int NumberOfCustomers;
        double[] Scoring;
        double[] Earnings;
        DateTime[] DateOfBirth;
        bool[] IsSmoking;
        // ...

        public void UpdateScorings()
        {
            for (int i = 0; i < NumberOfCustomers; ++i)
            {
                Scoring[i] = Earnings[i] * (IsSmoking[i] ? 0.8 : 1.0) * ProcessAge(DateOfBirth[i]);
            }
        }

        private double ProcessAge(DateTime dateTime) => 1.0;
    }

    class CustomerRepository2
    {
        int NumberOfCustomers;
        private HotCustomerData[] HotData;
        // ...
        public void UpdateScorings()
        {
            for (int i = 0; i < NumberOfCustomers; ++i)
            {
                HotData[i].Scoring = HotData[i].Earnings * (HotData[i].IsSmoking ? 0.8 : 1.0) * ProcessAge(HotData[i].DateOfBirth);
            }
        }

        private double ProcessAge(object p) => 1.0;
    }

    ///////////////////////////////////////////////////////////////////////
    // Listing 14-47
    [StructLayout(LayoutKind.Sequential)]
    struct CustomerValue
    {
        public double Earnings;
        public double Scoring;
        public long DateOfBirthInTicks;
        public bool IsSmoking;
        // ... some other fields ...
        public int HealthDataId;
        public int AuxiliaryDataId;
    }

    struct HotCustomerData
    {
        public double Scoring;
        public double Earnings;
        public DateTime DateOfBirth;
        public bool IsSmoking;
    }

    struct Customer
    {
        public HotCustomerData HotData;
        // ... some other fields ...
        public HealthData Health;
        public AuxiliaryData Auxiliary;

        public void UpdateScoring()
        {
            this.HotData.Scoring = this.HotData.Earnings * (this.HotData.IsSmoking ? 0.8 : 1.0) * ProcessAge(this.HotData.DateOfBirth);
        }

        private double ProcessAge(DateTime dateOfBirth) => 1.0;
    }

    public class HealthData
    {
    }

    public class AuxiliaryData
    {
    }
}
