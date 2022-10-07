namespace Xilium.Crdtp.Client.Logging;

/// <summary>
/// Message writer options.
/// By default it emit messages as is.
/// Always uses UTF-8 "Lax" encoding (e.g. non-throwing on invalid bytes).
/// </summary>
public struct CrdtpMessageWriterOptions
{
    public static CrdtpMessageWriterOptions CreateWithSuggestedStyle(bool ascii = false) =>
        new CrdtpMessageWriterOptions
        {
            SendPrefix = ascii ? ">>>|" : "|||>",    // "|J|>|"
            ReceivePrefix = ascii ? "<<<|" : "<|||", // "|<|J|"
            OverflowSizeInBytes = 4096,
            EllipsisString = ascii ? " ... " : " \x2026 ",
        };

    /// <summary>
    /// Prefix indicates message being send.
    /// </summary>
    public string? SendPrefix { get; set; }

    /// <summary>
    /// Prefix indicates received message.
    /// </summary>
    public string? ReceivePrefix { get; set; }

    /// <summary>
    /// If message (in bytes) is bigger than given size, then writer will
    /// try render text to fit specified size, by cutting text from center of
    /// message and inserting <see cref="EllipsisString"/>.
    ///
    /// When zero or negative - always write full message (e.g. no overflow).
    /// Size doesn't takes into account <see cref="SendPrefix"/> or
    /// <see cref="ReceivePrefix"/>.
    ///
    /// This only approximated size, underlying trimming happens at input bytes
    /// level, and not at final characters, so if input (UTF8) text heavy on
    /// multibyte characters, resulting text will be smaller. However, it handle
    /// input encoding correctly, when choosing trimming boundary.
    ///
    /// This size should have reasonable big value, because smallest protocol
    /// messages without payloads is bigger than 70 bytes.
    /// </summary>
    public int OverflowSizeInBytes { get; set; }

    /// <summary>
    /// Text inserted inside message, which indicates truncation.
    /// </summary>
    public string? EllipsisString { get; set; }
}
