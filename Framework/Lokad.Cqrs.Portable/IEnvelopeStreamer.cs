#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs
{
    /// <summary>
    /// Is responsible for reading-writing message envelopes either as
    /// data or references to data (in case envelope does not fit media)
    /// </summary>
    public interface IEnvelopeStreamer
    {
        /// <summary>
        /// Saves reference to message envelope as array of bytes.
        /// </summary>
        /// <param name="reference">The reference to to message envelope.</param>
        /// <returns>byte array that could be used to rebuild reference</returns>
        byte[] SaveEnvelopeReference(EnvelopeReference reference);
        /// <summary>
        /// Saves message envelope as array of bytes.
        /// </summary>
        /// <param name="envelope">The message envelope.</param>
        /// <returns></returns>
        byte[] SaveEnvelopeData(ImmutableEnvelope envelope);
        /// <summary>
        /// Tries the read buffer as reference to message envelope.
        /// </summary>
        /// <param name="buffer">The buffer to read.</param>
        /// <param name="reference">The reference to message envelope.</param>
        /// <returns><em>True</em> if we were able to read the reference; <em>false</em> otherwise</returns>
        bool TryReadAsEnvelopeReference(byte[] buffer, out EnvelopeReference reference);
        /// <summary>
        /// Reads the buffer as message envelope
        /// </summary>
        /// <param name="buffer">The buffer to read.</param>
        /// <returns>message envelope</returns>
        ImmutableEnvelope ReadAsEnvelopeData(byte[] buffer);
    }
}