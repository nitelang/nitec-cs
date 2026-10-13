namespace NiteCompiler.CodeAnalysis;

public enum SpecialType
{
	None = 0,
	SInt8 = 1,
	SInt16 = 2,
	SInt32 = 3,
	SInt64 = 4,
	SInt128 = 5,
	SNativeInt = 6,
	UInt8 = 7,
	UInt16 = 8,
	UInt32 = 9,
	UInt64 = 10,
	UInt128 = 11,
	UNativeInt = 12,
	Float16 = 13,
	Float32 = 14,
	Float64 = 15,
	NeverReturn = 16,
	Boolean = 17,
	CharUtf8 = 18,
	CharUtf16 = 19,
	CharUtf32 = 20,
}