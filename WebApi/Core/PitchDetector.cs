namespace WebApi.Core;

public class PitchDetector
{
    private readonly int sampleRate;
    private readonly int minLag;
    private readonly int maxLag;

    public PitchDetector(int sampleRate,
                         float minFreq = 80f,
                         float maxFreq = 400f)
    {
        this.sampleRate = sampleRate;

        minLag = (int)(sampleRate / maxFreq);
        maxLag = (int)(sampleRate / minFreq);
    }

    public float DetectPitch(float[] buffer)
    {
        int size = buffer.Length;

        float mean = 0f;
        for (int i = 0; i < size; i++)
            mean += buffer[i];

        mean /= size;

        for (int i = 0; i < size; i++)
            buffer[i] -= mean;

        float maxCorr = 0f;
        int bestLag = 0;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float sum = 0f;

            for (int i = 0; i < size - lag; i++)
                sum += buffer[i] * buffer[i + lag];

            if (sum > maxCorr)
            {
                maxCorr = sum;
                bestLag = lag;
            }
        }

        if (bestLag == 0)
            return 0f;

        return (float)sampleRate / bestLag;
    }
}
