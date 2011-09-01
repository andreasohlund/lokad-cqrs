#region (c) 2010-2011 Lokad CQRS - New BSD License 

// Copyright (c) Lokad SAS 2010-2011 (http://www.lokad.com)
// This code is released as Open Source under the terms of the New BSD Licence
// Homepage: http://lokad.github.com/lokad-cqrs/

#endregion

using System;

namespace Snippets.HttpEndpoint
{
    public class MouseStats
    {
        public int MessagesCount { get; set; }
        public int MessagesPerSecond { get; set; }
        public long Distance { get; set; }

        readonly int[] _circularBuffer = new int[60];

        public void RecordMessage()
        {
            _circularBuffer[DateTime.Now.Second] += 1;
        }

        public void RefreshStatistics()
        {
            // clears the opposite side of the message count tracking 
            // buffer
            var offset = DateTime.Now.Second;
            for (int i = 0; i < 20; i++)
            {
                var loc = (offset + 20) % 60;
                _circularBuffer[loc] = 0;
            }

            MessagesPerSecond = _circularBuffer[offset - 1];
        }
    }
}