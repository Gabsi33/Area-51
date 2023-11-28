using System;
using System.Collections.Generic;
using System.Threading;

public enum SecurityLevel
{
    Confidential,
    Secret,
    TopSecret
}

public enum Floor
{
    Ground,
    Secret,
    Experimental,
    TopSecret
}

public class Agent
{
    public string Name { get; set; }
    public SecurityLevel SecurityLevel { get; set; }

    public Agent(string name, SecurityLevel securityLevel)
    {
        Name = name;
        SecurityLevel = securityLevel;
    }

    public void CallElevator(Elevator elevator, Floor currentFloor, Floor destinationFloor)
    {
        elevator.RequestFloor(currentFloor, destinationFloor, this);
    }

    public void EnterElevator(Elevator elevator)
    {
        elevator.Enter(this);
    }

    public void ExitElevator(Elevator elevator)
    {
        elevator.Exit(this);
    }
}

public class Elevator
{
    private Floor currentFloor;
    private List<Agent> agentsInside;
    private bool[] floorButtonsEnabled;
    private bool[] elevatorButtonsEnabled;
    private object lockObject = new object();

    public Elevator()
    {
        currentFloor = Floor.Ground;
        agentsInside = new List<Agent>();
        floorButtonsEnabled = new bool[Enum.GetValues(typeof(Floor)).Length];
        elevatorButtonsEnabled = new bool[Enum.GetValues(typeof(Floor)).Length];
    }

    public void RequestFloor(Floor currentFloor, Floor destinationFloor, Agent agent)
    {
        lock (lockObject)
        {
            floorButtonsEnabled[(int)currentFloor] = false;
            elevatorButtonsEnabled[(int)destinationFloor] = true;

            if (this.currentFloor == currentFloor)
            {
                Enter(agent);
            }
            else
            {
                Console.WriteLine($"{agent.Name} is waiting for the elevator on floor {currentFloor}.");
                while (this.currentFloor != currentFloor) { }
                Enter(agent);
            }
        }
    }

    public void Enter(Agent agent)
    {
        lock (lockObject)
        {
            agentsInside.Add(agent);
            Console.WriteLine($"{agent.Name} entered the elevator on floor {currentFloor}.");
            floorButtonsEnabled[(int)currentFloor] = true;
        }
    }

    public void Exit(Agent agent)
    {
        lock (lockObject)
        {
            agentsInside.Remove(agent);
            Console.WriteLine($"{agent.Name} exited the elevator on floor {currentFloor}.");

            if (agentsInside.Count == 0)
            {
                elevatorButtonsEnabled[(int)currentFloor] = false;
            }
        }
    }

    public void MoveToFloor(Floor floor)
    {
        lock (lockObject)
        {
            int currentFloorIndex = (int)currentFloor;
            int destinationFloorIndex = (int)floor;

            while (currentFloorIndex != destinationFloorIndex)
            {
                Thread.Sleep(1000);
                currentFloorIndex += Math.Sign(destinationFloorIndex - currentFloorIndex);
                currentFloor = (Floor)currentFloorIndex;
                Console.WriteLine($"Elevator is moving to floor {currentFloor}...");
            }

            Console.WriteLine($"Elevator arrived at floor {currentFloor}.");
            floorButtonsEnabled[(int)currentFloor] = true;
            elevatorButtonsEnabled[(int)currentFloor] = false;
        }
    }

    public void OpenDoor(Agent agent)
    {
        lock (lockObject)
        {
            if (agentsInside.Count > 0)
            {
                Agent lowestSecurityAgent = agentsInside[0];
                foreach (Agent a in agentsInside)
                {
                    if (a.SecurityLevel < lowestSecurityAgent.SecurityLevel)
                    {
                        lowestSecurityAgent = a;
                    }
                }

                if (agent == lowestSecurityAgent)
                {
                    Console.WriteLine($"Door opens on floor {currentFloor} for {agent.Name} (Security Level: {agent.SecurityLevel}).");
                }
                else
                {
                    Console.WriteLine($"Door does not open on floor {currentFloor} for {agent.Name} (Security Level: {agent.SecurityLevel}).");
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Door opens on floor {currentFloor} for {agent.Name} (Security Level: {agent.SecurityLevel}).");
            }

            Thread.Sleep(2000);
            Console.WriteLine("Door closes.");
            Thread.Sleep(1000);
            Exit(agent);
        }
    }

    public void PressFloorButton(Floor floor)
    {
        lock (lockObject)
        {
            floorButtonsEnabled[(int)floor] = false;
            if (currentFloor != floor)
            {
                MoveToFloor(floor);
            }
            else
            {
                Console.WriteLine("Elevator is already on the requested floor.");
            }
        }
    }

    public void PressElevatorButton(Floor floor)
    {
        lock (lockObject)
        {
            elevatorButtonsEnabled[(int)floor] = false;
            OpenDoor(null);
        }
    }

    public void DisplayFloorButtonsStatus()
    {
        Console.WriteLine("Floor buttons status:");
        for (int i = 0; i < floorButtonsEnabled.Length; i++)
        {
            Console.WriteLine($"{(Floor)i}: {(floorButtonsEnabled[i] ? "Enabled" : "Disabled")}");
        }
        Console.WriteLine();
    }

    public void DisplayElevatorButtonsStatus()
    {
        Console.WriteLine("Elevator buttons status:");
        for (int i = 0; i < elevatorButtonsEnabled.Length; i++)
        {
            Console.WriteLine($"{(Floor)i}: {(elevatorButtonsEnabled[i] ? "Enabled" : "Disabled")}");
        }
        Console.WriteLine();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Elevator elevator = new Elevator();

        Agent agent1 = new Agent("Agent1", SecurityLevel.Confidential);
        Agent agent2 = new Agent("Agent2", SecurityLevel.Secret);
        Agent agent3 = new Agent("Agent3", SecurityLevel.TopSecret);

        Thread agent1Thread = new Thread(() =>
        {
            while (true)
            {
                Floor currentFloor = (Floor)new Random().Next(0, 4);
                Floor destinationFloor = (Floor)new Random().Next(0, 4);
                agent1.CallElevator(elevator, currentFloor, destinationFloor);
                Thread.Sleep(new Random().Next(2000, 5000));
            }
        });

        Thread agent2Thread = new Thread(() =>
        {
            while (true)
            {
                Floor currentFloor = (Floor)new Random().Next(0, 4);
                Floor destinationFloor = (Floor)new Random().Next(0, 4);
                agent2.CallElevator(elevator, currentFloor, destinationFloor);
                Thread.Sleep(new Random().Next(2000, 5000));
            }
        });

        Thread agent3Thread = new Thread(() =>
        {
            while (true)
            {
                Floor currentFloor = (Floor)new Random().Next(0, 4);
                Floor destinationFloor = (Floor)new Random().Next(0, 4);
                agent3.CallElevator(elevator, currentFloor, destinationFloor);
                Thread.Sleep(new Random().Next(2000, 5000));
            }
        });

        Thread elevatorThread = new Thread(() =>
        {
            while (true)
            {
                elevator.DisplayFloorButtonsStatus();
                elevator.DisplayElevatorButtonsStatus();
                Thread.Sleep(1000);
            }
        });

        agent1Thread.Start();
        agent2Thread.Start();
        agent3Thread.Start();
        elevatorThread.Start();

        Console.ReadLine();
    }
}