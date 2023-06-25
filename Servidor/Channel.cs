/// Copyright (c) 2023 | Joao Matos, Joao Fernandes, Ruben Lisboa.
/// Check the end of the file for the extended copyright notice.
///
/// Class that defines the structure of a communication channel.
/// Each channel has a unique ID and a list of clients that are currently
/// subscribed to it. Simmilar to the Pub/Sub messaging pattern.
///
/// Additionally, each channel contains metadata to help keep track
/// of it's relevance on the network.
///
/// Frequently used channals are cached in memory to improve performance.
/// Least used channels are kept in the database and loaded when needed.
///
/// A channel's relevance in the network is determined by the amount of
/// users it has, the amount of times it has been requested, and the last
/// time it was requested. Each of these factors have their own weight:
///
/// - Amount of users subscribed to the channel (0.5)
/// - Amount of times the channel has been requested (0.3)
/// - The last time the channel was requested (0.2)
/// 
/// The higher the relevance score, the more likely the channel is to be
/// cached in memory.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Servidor
{
    public class Channel
    {
        public readonly float _channelRelevanceDecay = 0.0001f;  // How much the channel's relevance decays over time
        public readonly float _channelRelevanceDecayRate = 0.1f; // How often the channel's relevance decays
        public readonly float _amountOfUsersWeight = 0.5f;       // How much the amount of users affects the channel's relevance
        public readonly float _requestCountWeight = 0.3f;        // How much the amount of requests affects the channel's relevance
        public readonly float _lastRequestWeight = 0.2f;         // How much the last request affects the channel's relevance

        public float _channelRelevance { get; set; } = 0;      // Channel relevance score

        public string _channelID { get; set; }                 // Unique channel ID
        public string _channelName { get; set; }               // Channel name
        public List<string> _clients { get; set; }             // List of clients subscribed to this channel
        public int _channelRequestCount { get; set; }          // Keeps track of how many times this channel has been used
        public DateTime _lastChannelRequest { get; set; }      // Keeps track of the last time this channel was used

        public Channel() { }

        public Channel(string ID, string name)
        {
            _channelID = ID;
            _channelName = name;
            _clients = new List<string>();
            _channelRequestCount = 0;
            _lastChannelRequest = DateTime.Now;
        }

        public Channel(string ID, string name, List<string> clients, int requestCount, DateTime lastRequest)
        {
            _channelID = ID;
            _channelName = name;
            _clients = clients;
            _channelRequestCount = requestCount;
            _lastChannelRequest = lastRequest;
        }

        public void AddClient(string clientID) { _clients.Add(clientID); }
        public void RemoveClient(string clientID) { _clients.Remove(clientID); }

        private void IncrementRequestCount() { _channelRequestCount++; }
        private void ResetRequestCount() { _channelRequestCount = 0; }
        private void UpdateLastRequest() { _lastChannelRequest = DateTime.Now; }

        public void UpdateChannelUsage()
        { 
            IncrementRequestCount();
            UpdateLastRequest();
            _channelRelevance = RateChannel(this);
        }

        public float GetChannelRelevance() { return RateChannel(this); }

        /// Gives the channel a relevance score based on the following
        /// factors:
        ///
        /// - Amount of users subscribed to the channel
        /// - Amount of times the channel has been requested
        /// - The last time the channel was requested
        ///
        /// (Amount of users * 0.5) + (Amount of requests * 0.3) + (Last request * 0.2)
        ///
        /// The relevance score is used to determine which channels are
        /// cached in memory and which are kept in the database. The higher
        /// the score, the more likely the channel is to be cached.
        private static float RateChannel(Channel channel)
        {
            float decayFactor = (float)Math.Pow(1f - channel._channelRelevanceDecay, TimeSinceLastRequest(channel) / channel._channelRelevanceDecayRate);

            float relevanceScore = (channel._amountOfUsersWeight * channel._clients.Count) +
                                   (channel._requestCountWeight * channel._channelRequestCount) +
                                   (channel._lastRequestWeight * decayFactor);

            return relevanceScore;
        }

        /// Calculates the time elapsed since the last time the channel
        /// was requested. Used to determine the channel's relevance and
        /// to decay it's relevance over time.
        private static float TimeSinceLastRequest(Channel channel)
        {
            TimeSpan timeSinceLastRequest = DateTime.Now - channel._lastChannelRequest;
            return (float)timeSinceLastRequest.TotalSeconds;
        }
    }
}

/// MIT License
/// 
/// Copyright (c) 2023 | João Matos, Joao Fernandes, Ruben Lisboa.
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.