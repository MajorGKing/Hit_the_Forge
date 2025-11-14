using System;
using System.Collections.Generic;

public class EventManager
{
	private Dictionary<Define.EEventType, Action> _events = new Dictionary<Define.EEventType, Action>();

	public void AddEvent(Define.EEventType eventType, Action listener)
	{
		if (_events.ContainsKey(eventType) == false)
			_events.Add(eventType, new Action(() => { }));

		_events[eventType] += listener;
	}

	public void RemoveEvent(Define.EEventType eventType, Action listener)
	{
        if (_events.ContainsKey(eventType))
			_events[eventType] -= listener;
	}

	public void TriggerEvent(Define.EEventType eventType)
	{
		if (_events.ContainsKey(eventType))
			_events[eventType].Invoke();
	}

	public void Clear()
	{
		_events.Clear();
	}

}
