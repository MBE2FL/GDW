/// ######### Conditional Functions #########
float4 when_eq_float4(float4 x, float4 y)
{
    return 1.0 - abs(sign(x - y));
}

float when_eq_float(float x, float y)
{
    return 1.0 - abs(sign(x - y));
}

int when_eq_int(int x, int y)
{
    return 1 - abs(sign(x - y));
}

int when_gt_int(int x, int y)
{
    return max(sign(x - y), 0);
}

float when_gt_float(float x, float y)
{
    return max(sign(x - y), 0.0);
}

float4 when_gt_float4(float4 x, float4 y)
{
    return max(sign(x - y), 0.0);
}

int4 when_gt_int(int4 x, int4 y)
{
    return max(sign(x - y), 0);
}

float when_lt_float(float x, float y)
{
    return max(sign(y - x), 0);
}

float4 when_lt_float(float4 x, float4 y)
{
    return max(sign(y - x), 0.0);
}

int4 when_lt_int(int4 x, int4 y)
{
    return max(sign(y - x), 0);
}

float when_ge_float(float x, float y)
{
    return 1.0 - when_lt_float(x, y);
}

float4 when_ge_float(float4 x, float4 y)
{
    return 1.0 - when_lt_float(x, y);
}

int4 when_ge_int(int4 x, int4 y)
{
    return 1 - when_lt_float(x, y);
}

float when_le_float(float x, float y)
{
    return 1.0 - when_gt_float(x, y);
}
/// ######### Conditional Functions #########