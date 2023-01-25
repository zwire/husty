#ifdef __cplusplus
#define DLLEXPORT extern "C" __declspec(dllexport)
#else
#define DLLEXPORT __declspec(dllexport)
#endif

#include "LogitechSteeringWheelLib.h"

DLLEXPORT bool __stdcall SteeringInitialize(bool ignoreXInputControllers)
{
	return LogiSteeringInitialize(ignoreXInputControllers);
}

DLLEXPORT bool __stdcall Update()
{
	return LogiUpdate();
}

DLLEXPORT DIJOYSTATE2ENGINES* __stdcall GetStateENGINES(const int index)
{
	return LogiGetStateENGINES(index);
}

DLLEXPORT bool __stdcall GetDevicePath(const int index, wchar_t* buffer, int bufferSize)
{
	return LogiGetDevicePath(index, buffer, bufferSize);
}

DLLEXPORT bool __stdcall GetFriendlyProductName(const int index, wchar_t* buffer, int bufferSize)
{
	return LogiGetFriendlyProductName(index, buffer, bufferSize);
}

DLLEXPORT bool __stdcall IsConnected(const int index)
{
	return LogiIsConnected(index);
}

DLLEXPORT bool __stdcall IsDeviceConnected(const int index, const int deviceType)
{
	return LogiIsDeviceConnected(index, deviceType);
}

DLLEXPORT bool __stdcall IsManufacturerConnected(const int index, const int manufacturerName)
{
	return LogiIsManufacturerConnected(index, manufacturerName);
}

DLLEXPORT bool __stdcall IsModelConnected(const int index, const int modelName)
{
	return LogiIsModelConnected(index, modelName);
}

DLLEXPORT bool __stdcall ButtonTriggered(const int index, const int buttonNbr)
{
	return LogiButtonTriggered(index, buttonNbr);
}

DLLEXPORT bool __stdcall ButtonReleased(const int index, const int buttonNbr)
{
	return LogiButtonReleased(index, buttonNbr);
}

DLLEXPORT bool __stdcall ButtonIsPressed(const int index, const int buttonNbr)
{
	return LogiButtonIsPressed(index, buttonNbr);
}

DLLEXPORT bool __stdcall GenerateNonLinearValues(const int index, const int nonLinCoeff)
{
	return LogiGenerateNonLinearValues(index, nonLinCoeff);
}

DLLEXPORT int __stdcall GetNonLinearValue(const int index, const int inputValue)
{
	return LogiGetNonLinearValue(index, inputValue);
}

DLLEXPORT bool __stdcall HasForceFeedback(const int index)
{
	return LogiHasForceFeedback(index);
}

DLLEXPORT bool __stdcall IsPlaying(const int index, const int forceType)
{
	return LogiIsPlaying(index, forceType);
}

DLLEXPORT bool __stdcall PlaySpringForce(const int index, const int offsetPercentage, const int saturationPercentage, const int coefficientPercentage)
{
	return LogiPlaySpringForce(index, offsetPercentage, saturationPercentage, coefficientPercentage);
}

DLLEXPORT bool __stdcall StopSpringForce(const int index)
{
	return LogiStopSpringForce(index);
}

DLLEXPORT bool __stdcall PlayConstantForce(const int index, const int magnitudePercentage)
{
	return LogiPlayConstantForce(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall StopConstantForce(const int index)
{
	return LogiStopConstantForce(index);
}

DLLEXPORT bool __stdcall PlayDamperForce(const int index, const int coefficientPercentage)
{
	return LogiPlayDamperForce(index, coefficientPercentage);
}

DLLEXPORT bool __stdcall StopDamperForce(const int index)
{
	return LogiStopDamperForce(index);
}

DLLEXPORT bool __stdcall PlaySideCollisionForce(const int index, const int magnitudePercentage)
{
	return LogiPlaySideCollisionForce(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall PlayFrontalCollisionForce(const int index, const int magnitudePercentage)
{
	return LogiPlayFrontalCollisionForce(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall PlayDirtRoadEffect(const int index, const int magnitudePercentage)
{
	return LogiPlayDirtRoadEffect(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall StopDirtRoadEffect(const int index)
{
	return LogiStopDirtRoadEffect(index);
}

DLLEXPORT bool __stdcall PlayBumpyRoadEffect(const int index, const int magnitudePercentage)
{
	return LogiPlayBumpyRoadEffect(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall StopBumpyRoadEffect(const int index)
{
	return LogiStopBumpyRoadEffect(index);
}

DLLEXPORT bool __stdcall PlaySlipperyRoadEffect(const int index, const int magnitudePercentage)
{
	return LogiPlaySlipperyRoadEffect(index, magnitudePercentage);
}

DLLEXPORT bool __stdcall StopSlipperyRoadEffect(const int index)
{
	return LogiStopSlipperyRoadEffect(index);
}

DLLEXPORT bool __stdcall PlaySurfaceEffect(const int index, const int type, const int magnitudePercentage, const int period)
{
	return LogiPlaySurfaceEffect(index, type, magnitudePercentage, period);
}

DLLEXPORT bool __stdcall StopSurfaceEffect(const int index)
{
	return LogiStopSurfaceEffect(index);
}

DLLEXPORT bool __stdcall PlayCarAirborne(const int index)
{
	return LogiPlayCarAirborne(index);
}

DLLEXPORT bool __stdcall StopCarAirborne(const int index)
{
	return LogiStopCarAirborne(index);
}

DLLEXPORT bool __stdcall PlaySoftstopForce(const int index, const int usableRangePercentage)
{
	return LogiPlaySoftstopForce(index, usableRangePercentage);
}

DLLEXPORT bool __stdcall StopSoftstopForce(const int index)
{
	return LogiStopSoftstopForce(index);
}

DLLEXPORT bool __stdcall SetPreferredControllerProperties(const LogiControllerPropertiesData properties)
{
	return LogiSetPreferredControllerProperties(properties);
}

DLLEXPORT bool __stdcall GetCurrentControllerProperties(const int index, LogiControllerPropertiesData& properties)
{
	return LogiGetCurrentControllerProperties(index, properties);
}

DLLEXPORT int __stdcall GetShifterMode(const int index)
{
	return LogiGetShifterMode(index);
}

DLLEXPORT bool __stdcall SetOperatingRange(const int index, const int range)
{
	return LogiSetOperatingRange(index, range);
}

DLLEXPORT bool __stdcall GetOperatingRange(const int index, int& range)
{
	return LogiGetOperatingRange(index, range);
}

DLLEXPORT bool __stdcall PlayLeds(const int index, const float currentRPM, const float rpmFirstLedTurnsOn, const float rpmRedLine)
{
	return LogiPlayLeds(index, currentRPM, rpmFirstLedTurnsOn, rpmRedLine);
}

DLLEXPORT void __stdcall SteeringShutdown()
{
	LogiSteeringShutdown();
}
