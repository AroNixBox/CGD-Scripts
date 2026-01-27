// Fade Kurve
float fade(float t)
{
    return t * t * t * (t * (t * 6 - 15) + 10);
}

// Pseudo-Permutation Hash
int perm(int x)
{
    return int(frac(sin(float(x) * 127.1) * 43758.5453) * 256.0);
}

// Grad Funktion
float grad(int hash, float x, float y)
{
    int h = hash & 3;
    float u = h < 2 ? x : y;
    float v = h < 2 ? y : x;
    return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
}

// Perlin Noise 2D
float perlin(float2 p)
{
    float2 pi = floor(p);
    float2 pf = p - pi;

    int xi = (int)pi.x & 255;
    int yi = (int)pi.y & 255;

    int aa = perm(xi + perm(yi));
    int ab = perm(xi + perm(yi + 1));
    int ba = perm(xi + 1 + perm(yi));
    int bb = perm(xi + 1 + perm(yi + 1));

    float2 f = float2(fade(pf.x), fade(pf.y));

    float x1 = lerp(grad(aa, pf.x, pf.y), grad(ba, pf.x - 1.0, pf.y), f.x);
    float x2 = lerp(grad(ab, pf.x, pf.y - 1.0), grad(bb, pf.x - 1.0, pf.y - 1.0), f.x);

    return lerp(x1, x2, f.y);
}

// FBM für Shader Graph
void fbm_float(float2 UV, int Octaves, float Lacunarity, float Roughness, out float value)
{
    value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;

    for (int i = 0; i < Octaves; ++i)
    {
        value += amplitude * perlin(UV * frequency);
        frequency *= Lacunarity;
        amplitude *= Roughness;
    }

    value = value * 0.5 + 0.5; // normalize to [0,1]
}
