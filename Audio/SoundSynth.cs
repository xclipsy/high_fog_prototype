using Microsoft.Xna.Framework.Audio;

namespace HighFog;

/// <summary>
/// Real-time procedural audio synthesis engine.
/// Generates atmospheric survival horror audio waveforms (wind, gunshots, screams, footsteps, UI sounds)
/// directly in code without requiring external audio asset files.
/// </summary>
public sealed class SoundSynth : IDisposable
{
    private const int SampleRate = 44100;
    private readonly DynamicSoundEffectInstance _ambienceStream;
    private readonly byte[] _ambienceBuffer;
    private readonly Random _rand = new(42);
    private float _ambiencePhase;
    private float _windGustPhase;
    private bool _ambienceRunning;

    public SoundSynth()
    {
        // Continuous ambient wind stream (Mono 44.1kHz 16-bit)
        _ambienceStream = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
        _ambienceBuffer = new byte[SampleRate / 4 * 2]; // 250ms buffer chunks
        _ambienceStream.BufferNeeded += OnAmbienceBufferNeeded;
    }

    public void StartAmbience()
    {
        if (_ambienceRunning) return;
        _ambienceRunning = true;
        SubmitAmbienceChunk();
        SubmitAmbienceChunk();
        _ambienceStream.Play();
        _ambienceStream.Volume = 0.45f;
    }

    public void StopAmbience()
    {
        _ambienceRunning = false;
        _ambienceStream.Stop();
    }

    public void SetAmbienceVolume(float volume)
    {
        _ambienceStream.Volume = Math.Clamp(volume, 0f, 1f);
    }

    private void OnAmbienceBufferNeeded(object? sender, EventArgs e)
    {
        if (_ambienceRunning)
        {
            SubmitAmbienceChunk();
        }
    }

    private void SubmitAmbienceChunk()
    {
        int sampleCount = _ambienceBuffer.Length / 2;
        float lastVal = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            _ambiencePhase += 1f / SampleRate;
            _windGustPhase += 0.2f / SampleRate;
            
            // Slow undulating gust envelope (0.2 to 0.8)
            float gust = 0.35f + 0.25f * MathF.Sin(_windGustPhase * 1.7f) + 0.15f * MathF.Sin(_windGustPhase * 3.3f);
            
            // Low-pass filtered noise for cold hollow wind
            float white = (float)(_rand.NextDouble() * 2.0 - 1.0);
            lastVal = lastVal * 0.92f + white * 0.08f;
            
            // Subtle eerie low frequency drone (68 Hz + 102 Hz)
            float drone = MathF.Sin(_ambiencePhase * 68f * MathF.Tau) * 0.12f +
                          MathF.Sin(_ambiencePhase * 102f * MathF.Tau) * 0.08f;

            float sample = (lastVal * gust + drone) * 0.4f;
            sample = Math.Clamp(sample, -1f, 1f);
            
            short pcm = (short)(sample * short.MaxValue);
            _ambienceBuffer[i * 2] = (byte)(pcm & 0xFF);
            _ambienceBuffer[i * 2 + 1] = (byte)((pcm >> 8) & 0xFF);
        }
        _ambienceStream.SubmitBuffer(_ambienceBuffer);
    }

    public void PlayGunshot()
    {
        int samples = (int)(SampleRate * 0.45f);
        var pcm = new byte[samples * 2];
        var rand = new Random();
        float lp = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            // Quick explosive burst
            float env = MathF.Exp(-t * 14f);
            // Low boom kick (starts at 180Hz dropping to 40Hz)
            float freq = MathF.Max(40f, 180f * MathF.Exp(-t * 22f));
            float boom = MathF.Sin(t * freq * MathF.Tau) * env;
            
            // Crack noise
            float noise = ((float)rand.NextDouble() * 2f - 1f) * MathF.Exp(-t * 28f);
            lp = lp * 0.7f + noise * 0.3f;

            float outSample = (boom * 0.65f + lp * 0.55f);
            outSample = Math.Clamp(outSample, -1f, 1f);

            short val = (short)(outSample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayDryFire()
    {
        int samples = (int)(SampleRate * 0.08f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 80f);
            float click = MathF.Sin(t * 1800f * MathF.Tau) * env * 0.5f;
            short val = (short)(click * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayReload()
    {
        int samples = (int)(SampleRate * 0.7f);
        var pcm = new byte[samples * 2];
        var rand = new Random();
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float sample = 0f;
            
            // Click 1 (Magazine eject at 0.1s)
            if (t > 0.08f && t < 0.2f)
            {
                float lt = t - 0.08f;
                sample += MathF.Sin(lt * 1200f * MathF.Tau) * MathF.Exp(-lt * 60f) * 0.4f;
            }
            // Click 2 (Magazine insert at 0.38s)
            if (t > 0.35f && t < 0.5f)
            {
                float lt = t - 0.35f;
                sample += MathF.Sin(lt * 850f * MathF.Tau) * MathF.Exp(-lt * 50f) * 0.6f;
            }
            // Slide rack at 0.55s
            if (t > 0.52f)
            {
                float lt = t - 0.52f;
                float noise = ((float)rand.NextDouble() * 2f - 1f) * 0.3f;
                sample += (MathF.Sin(lt * 1600f * MathF.Tau) * 0.4f + noise) * MathF.Exp(-lt * 45f);
            }

            sample = Math.Clamp(sample, -1f, 1f);
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayFootstep(bool running)
    {
        int samples = (int)(SampleRate * 0.12f);
        var pcm = new byte[samples * 2];
        var rand = new Random();
        float pitch = 90f + rand.Next(-15, 16);
        float volume = running ? 0.32f : 0.2f;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 40f);
            float thud = MathF.Sin(t * pitch * MathF.Tau) * env;
            float grit = ((float)rand.NextDouble() * 2f - 1f) * MathF.Exp(-t * 60f) * 0.25f;
            float sample = (thud + grit) * volume;
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayMonsterGrowl()
    {
        int samples = (int)(SampleRate * 1.1f);
        var pcm = new byte[samples * 2];
        var rand = new Random();

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Sin(t / 1.1f * MathF.PI); // Arc envelope
            // Distorted FM rasp
            float mod = MathF.Sin(t * 34f * MathF.Tau) * 45f;
            float carrier = MathF.Sin(t * (75f + mod) * MathF.Tau);
            float noise = ((float)rand.NextDouble() * 2f - 1f) * 0.25f;
            float sample = (carrier * 0.6f + noise) * env * 0.45f;
            short val = (short)(Math.Clamp(sample, -1f, 1f) * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayMonsterScreech()
    {
        int samples = (int)(SampleRate * 0.85f);
        var pcm = new byte[samples * 2];
        var rand = new Random();

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 3.5f) * MathF.Min(1f, t * 25f);
            // Gliding high pitch screech (900Hz to 400Hz)
            float freq = 850f * MathF.Exp(-t * 1.8f) + 150f;
            float fmM = MathF.Sin(t * 120f * MathF.Tau) * 120f;
            float scream = MathF.Sin(t * (freq + fmM) * MathF.Tau);
            float grit = ((float)rand.NextDouble() * 2f - 1f) * 0.35f;
            float sample = (scream * 0.5f + grit) * env * 0.5f;
            short val = (short)(Math.Clamp(sample, -1f, 1f) * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayMonsterHit()
    {
        int samples = (int)(SampleRate * 0.25f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 18f);
            float squish = MathF.Sin(t * 220f * MathF.Tau) * env * 0.5f;
            short val = (short)(squish * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayPlayerHurt()
    {
        int samples = (int)(SampleRate * 0.35f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 12f);
            float grunt = MathF.Sin(t * 95f * MathF.Tau) * env * 0.55f;
            short val = (short)(grunt * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayItemPickup()
    {
        // Classic survival horror two-tone discovery chime
        int samples = (int)(SampleRate * 0.7f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float tone1 = MathF.Sin(t * 587.33f * MathF.Tau) * MathF.Exp(-t * 4.5f); // D5
            float tone2 = 0f;
            if (t > 0.14f)
            {
                float lt = t - 0.14f;
                tone2 = MathF.Sin(lt * 880f * MathF.Tau) * MathF.Exp(-lt * 4.0f); // A5
            }
            float sample = (tone1 * 0.35f + tone2 * 0.45f) * 0.5f;
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayDoor()
    {
        int samples = (int)(SampleRate * 0.45f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float env = MathF.Exp(-t * 8f);
            // Squeak + heavy latch thud
            float squeak = MathF.Sin(t * (420f - t * 300f) * MathF.Tau) * 0.25f;
            float thud = (t > 0.2f) ? MathF.Sin((t - 0.2f) * 80f * MathF.Tau) * MathF.Exp(-(t - 0.2f) * 25f) * 0.5f : 0f;
            float sample = (squeak * env + thud) * 0.5f;
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayUiBlip()
    {
        int samples = (int)(SampleRate * 0.05f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float sample = MathF.Sin(t * 700f * MathF.Tau) * MathF.Exp(-t * 90f) * 0.25f;
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayUiSelect()
    {
        int samples = (int)(SampleRate * 0.1f);
        var pcm = new byte[samples * 2];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float sample = MathF.Sin(t * 1100f * MathF.Tau) * MathF.Exp(-t * 50f) * 0.3f;
            short val = (short)(sample * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    public void PlayPageTurn()
    {
        int samples = (int)(SampleRate * 0.18f);
        var pcm = new byte[samples * 2];
        var rand = new Random();
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float noise = ((float)rand.NextDouble() * 2f - 1f) * MathF.Exp(-t * 22f) * 0.35f;
            short val = (short)(noise * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xFF);
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        PlayRawPcm(pcm);
    }

    private static void PlayRawPcm(byte[] buffer)
    {
        try
        {
            var sfx = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
            sfx.SubmitBuffer(buffer);
            sfx.Play();
        }
        catch
        {
            // Fallback gracefully if audio device is unavailable
        }
    }

    public void Dispose()
    {
        _ambienceStream.Dispose();
    }
}
