using System;
using xs.concurrent;					

namespace SantaPlaces
{
	concurrent class Santa
	{
		bool _meeting = false;
		bool _delivering = false;
		public void meeting()
		{
			if (_delivering)
				await finishedDelivering;

			_meeting = true;
				seconds(3, 5) | (cancelMeeting >> meetingCanceled());
			_meeting = false;
		}

		public void deliver()
		{
			Debug.Assert(!_delivering);
			_delivering = true;

			if (_meeting)
				cancelMeeting() >> meetingCanceled;

			await seconds(20, 30);

			//christmas is over
			_delivering = false;
			finishedDelivering();
		}
	}
}
