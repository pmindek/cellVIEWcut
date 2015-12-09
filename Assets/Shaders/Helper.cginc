 //--------------------------------------------------------------------------------------

float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return float3(H, C, Q.x);
}

float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R,G,B));
}

////--------------------------------------------------------------------------------------

float3 RGBtoHSL(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float L = HCV.z - HCV.y * 0.5;
	float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
	return float3(HCV.x, S, L);
}

float3 HSLtoRGB(in float3 HSL)
{
	float3 RGB = HUEtoRGB(HSL.x);
	float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
	return (RGB - 0.5) * C + HSL.z;
}

//--------------------------------------------------------------------------------------

float3 RGBtoHSV(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return float3(HCV.x, S, HCV.z);
}

float3 HSVtoRGB(in float3 HSV)
{
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

//--------------------------------------------------------------------------------------

float3 SetHSV(float3 color, float3 hsv)
{
	float3 c = RGBtoHSV(color);		
		
	c.x = (hsv.x < 0) ? c.x : hsv.x;
	c.y = (hsv.y < 0) ? c.y : hsv.y;
	c.z = (hsv.z < 0) ? c.z : hsv.z;

	return 	HSVtoRGB(c);	
}

float3 OffsetHSV(float3 color, float3 offset)
{
	float3 c = RGBtoHSV(color);		
	return 	HSVtoRGB(c + offset);	
}
		
//--------------------------------------------------------------------------------------

float3 ColorCorrection(float3 color)
{
	float3 c = RGBtoHSL(color);		
	
	c.z = 0.5;
	c.y = 0.65;

	return 	HSLtoRGB(c);	
}

float3 HighlightColor(float3 color)
{
	float3 c = RGBtoHSL(color);		
	
	c.y = 1.0;
	c.z = 0.5;

	return 	HSLtoRGB(c);	
}

float3 SetHSL(float3 color, float3 hsl)
{
	float3 c = RGBtoHSL(color);		
		
	c.x = (hsl.x < 0) ? c.x : hsl.x;
	c.y = (hsl.y < 0) ? c.y : hsl.y;
	c.z = (hsl.z < 0) ? c.z : hsl.z;

	return 	HSLtoRGB(c);		
}
	
float3 OffsetHSL(float3 color, float3 offset)
{
	float3 c = RGBtoHSL(color);		
	return 	HSLtoRGB(c + offset);		
}	

//--------------------------------------------------------------------------------------

float3 QuaternionTransform( float4 q, float3 v )
{ 
	float3 t = 2 * cross(q.xyz, v);
	return v + q.w * t + cross(q.xyz, t);
}

float4 QuaternionFromAxisAngle(float3 axis, float angle)
{
	float4 q;
	q.x = axis.x * sin(angle/2);
	q.y = axis.y * sin(angle/2);
	q.z = axis.z * sin(angle/2);
	q.w = cos(angle/2);
	return q;
}

float3 CubicInterpolate(float3 y0, float3 y1, float3 y2,float3 y3, float mu)
{
   float mu2 = mu*mu;
   float3 a0,a1,a2,a3;

   a0 = y3 - y2 - y0 + y1;
   a1 = y0 - y1 - a0;
   a2 = y2 - y0;
   a3 = y1;

   return(a0*mu*mu2 + a1*mu2+a2 * mu+a3);
}

//-----------------------------------------------------------------------------------

float4 ComputePlane(float3 normal, float3 inPoint)
{
	return float4(normalize(normal), -dot(normal, inPoint));
}

bool SpherePlaneTest(float4 plane, float4 sphere)
{
	return dot(plane.xyz, sphere.xyz - plane.xyz * -plane.w) + sphere.w > 0;
}

bool SphereFrustrumTest( float4 frustrumPlanes[6], float4 sphere)
{
	bool inFrustrum = true;

	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[0], sphere);
	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[1], sphere);
	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[2], sphere);
	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[3], sphere);
	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[4], sphere);
	inFrustrum = inFrustrum & SpherePlaneTest(frustrumPlanes[5], sphere);	

	return !inFrustrum;
}

bool SphereSphereTest(float4 sphere, float4 atom)
{
	return (length(sphere.xyz - atom.xyz) <= sphere.w);
}

bool SphereCubeTest(float3 position, float4 rotation, float3 size, float4 sphere)
{

	float3 d = abs(QuaternionTransform(float4(-rotation.x, -rotation.y, -rotation.z, rotation.w), sphere.xyz - position.xyz)) - size.xyz;
	return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0)) <= 0;
}

//-----------------------------------------------------------------------------------

float SpherePlaneSD(float4 plane, float4 sphere)
{
	return -(dot(plane.xyz, sphere.xyz - plane.xyz * -plane.w) + sphere.w);
}

float SphereSphereSD(float3 position, float4 rotation, float3 size, float4 sphere)
{
	return (length( QuaternionTransform(float4(-rotation.x, -rotation.y, -rotation.z, rotation.w), sphere.xyz - position.xyz) / size ) - 1.0) * min(min(size.x,size.y),size.z);

	//return (length( (sphere.xyz - atom.xyz) / size ) - 1.0) * min(min(size.x,size.y),size.z);

	//return length(sphere.xyz - atom.xyz * size.xyz) - sphere.w;
}

float SphereCubeSD(float3 position, float4 rotation, float3 size, float4 sphere)
{
	float3 d = abs(QuaternionTransform(float4(-rotation.x, -rotation.y, -rotation.z, rotation.w), sphere.xyz - position.xyz)) - size.xyz;
	return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}

float SphereCylinderSD(float3 position, float4 rotation, float3 size, float4 sphere)
{
	float3 t = QuaternionTransform(float4(-rotation.x, -rotation.y, -rotation.z, rotation.w), sphere.xyz - position.xyz);

	float2 d = abs(float2(length(t.xz),t.y)) - size.xy;
	return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}

float SphereInfiniteConeSD(float3 position, float4 rotation, float3 size, float4 sphere)
{
	float3 t = QuaternionTransform(float4(-rotation.x, -rotation.y, -rotation.z, rotation.w), sphere.xyz - position.xyz);

    float q = length(t.xy);
    return dot(size.xy, float2(q, t.z));
}

uint wang_hash(uint seed)
{
    seed = (seed ^ 61) ^ (seed >> 16);
    seed *= 9;
    seed = seed ^ (seed >> 4);
    seed *= 0x27d4eb2d;
    seed = seed ^ (seed >> 15);
    return seed;
}

float rand(uint value)
{
	return float(wang_hash(value)) * (1.0 / 4294967296.0);
}

bool AdjustDistanceTest(in int id, in float parameter, in float distance, in float pSize, in float pFuzziness, in float pFuzzinessCurve)
{
	float normalizedDistance = pow(abs(distance) / (0.01 + pSize * 250.0), pFuzzinessCurve);

	if (normalizedDistance > 1.0)
	{
		return true;
	}
	else
	{
		return rand(id) < 1.0 - parameter * lerp(1.0, pFuzziness, pow(normalizedDistance, 1.0));
	}
	return true;
}

float GetSignedDistance(float3 sphere, int type, float3 position, float4 rotation, float3 scale)
{
	float distance = 0.0;

    if (type == 0)
    {
	    float3 normal = QuaternionTransform(rotation, float3(0, 1, 0));
        float4 plane = ComputePlane(normal, position);
        distance = SpherePlaneSD(plane, float4(sphere.xyz, 0));
    }
	else if (type == 1)
	{
		distance = SphereSphereSD(position, rotation, scale * 0.5, float4(sphere.xyz, 0));
	}
	else if (type == 2)
	{
		distance = SphereCubeSD(position, rotation, scale * 0.50, float4(sphere.xyz, 0));
	}
	else if (type == 3)
	{
		distance = SphereCylinderSD(position, rotation, scale * 0.50, float4(sphere.xyz, 0));
	}
	else if (type == 4)
	{
		distance = SphereInfiniteConeSD(position, rotation, scale * 0.5, float4(sphere.xyz, 0));
	}
	else if (type == 5)
	{
		distance = 0.1;
	}

	return distance;
}