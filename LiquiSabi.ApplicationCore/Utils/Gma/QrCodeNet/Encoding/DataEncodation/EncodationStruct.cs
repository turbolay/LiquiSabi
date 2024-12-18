using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.Versions;

namespace LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.DataEncodation;

internal struct EncodationStruct
{
	internal EncodationStruct(VersionControlStruct vcStruct, BitList dataCodewords)
	{
		VersionDetail = vcStruct.VersionDetail;
		DataCodewords = dataCodewords;
	}

	internal VersionDetail VersionDetail { get; set; }
	internal BitList DataCodewords { get; set; }
}
