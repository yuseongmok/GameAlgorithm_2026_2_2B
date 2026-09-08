using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public sealed class WarehouseData : MonoBehaviour, IWarehouseData
{
    //Incoming / outgoing 은 먼저 들어온 상자가 먼저 나가는 Queue(FIF0)

    private readonly Queue<ForkliftBox> incomingQueue = new Queue<ForkliftBox>();

    private readonly Queue<ForkliftBox> outgoingQueue = new Queue<ForkliftBox>();

    //전재 공간은 나중에 올린 상자를 먼저 꺼내는 Stack(LIF0)
    //stackIndex 마다 하나의 stack를 관리 한다.

    private readonly Dictionary<int, Stack<ForkliftBox>> stacks = new Dictionary<int, Stack<ForkliftBox>>();

    public int IncomingCount => incomingQueue.Count;

    public int OutgoingCount => outgoingQueue.Count;

    public void Clear()
    {
        incomingQueue.Clear();
        outgoingQueue.Clear();
        stacks.Clear();
    }

    //Incoming Queue
    public void EnqueueIncoming(ForkliftBox box)
    {
        if (box == null)
        {
            return;
        }

        incomingQueue.Enqueue(box);
    }

    public ForkliftBox PeekIncoming()
    {
        if (incomingQueue.Count == 0)
        {
            return null;
        }

        return incomingQueue.Peek();
    }

    public ForkliftBox DequeueIncoming()
    {
        if (incomingQueue.Count == 0)
        {
            return null;
        }

        return incomingQueue.Dequeue();
    }

    public ForkliftBox[] GetIncomingItems()
    {
        return incomingQueue.ToArray();
    }

    //Outgoing Queue
    public void EnqueueOutgoing(ForkliftBox box)
    {
        if (box == null)
        {
            return;
        }

        outgoingQueue.Enqueue(box);
    }

    public ForkliftBox PeekOutgoing()
    {
        if (outgoingQueue.Count == 0)
        {
            return null;
        }

        return outgoingQueue.Peek();
    }

    public ForkliftBox DequeueOutgoing()
    {
        if (outgoingQueue.Count == 0)
        {
            return null;
        }

        return outgoingQueue.Dequeue();
    }

    public ForkliftBox[] GetOutgoingItems()
    {
        return outgoingQueue.ToArray();
    }

    //Stack

    //해당 번호의 stack 이 없으면 새로 만들어서 변환

    private Stack<ForkliftBox> GetOrCreateStack(int stackIndex)
    {
        if (!stacks.TryGetValue(stackIndex, out Stack<ForkliftBox> stack))
        {
            stack = new Stack<ForkliftBox>();
            stacks.Add(stackIndex, stack);
        }

        return stack;
    }

    public int GetStackCount(int stackIndex)
    {
        return GetOrCreateStack(stackIndex).Count;
    }

    public void PushStack(int stackIndex, ForkliftBox box)
    {
        if(box == null)
        {
            return;
        }

        GetOrCreateStack(stackIndex).Push(box);
    }

    public ForkliftBox PeekStack(int stackIndex)
    {
        Stack<ForkliftBox> stack = GetOrCreateStack(stackIndex);
        if(stack.Count == 0)
        {
            return null;
        } 
        return stack.Peek();
    }

    public ForkliftBox PopStack(int stackIndex)
    {
        Stack<ForkliftBox> stack = GetOrCreateStack(stackIndex);
        if(stack.Count == 0)
        {
            return null;
        }
        return stack.Pop();
    }
}
