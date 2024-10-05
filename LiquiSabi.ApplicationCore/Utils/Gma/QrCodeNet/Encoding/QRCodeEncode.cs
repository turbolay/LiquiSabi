using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.DataEncodation;
using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.EncodingRegion;
using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.ErrorCorrection;
using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.Masking;
using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.Masking.Scoring;
using LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding.Positioning;

namespace LiquiSabi.ApplicationCore.Utils.Gma.QrCodeNet.Encoding;

internal static class QRCodeEncode
{
	internal static BitMatrix Encode(string content, ErrorCorrectionLevel errorLevel)
	{
		EncodationStruct encodeStruct = DataEncode.Encode(content, errorLevel);

		return ProcessEncodationResult(encodeStruct, errorLevel);
	}

	private static BitMatrix ProcessEncodationResult(EncodationStruct encodeStruct, ErrorCorrectionLevel errorLevel)
	{
		BitList codewords = ECGenerator.FillECCodewords(encodeStruct.DataCodewords, encodeStruct.VersionDetail);

		TriStateMatrix triMatrix = new(encodeStruct.VersionDetail.MatrixWidth);
		PositioningPatternBuilder.EmbedBasicPatterns(encodeStruct.VersionDetail.Version, triMatrix);

		triMatrix.EmbedVersionInformation(encodeStruct.VersionDetail.Version);
		triMatrix.EmbedFormatInformation(errorLevel, new Pattern0());
		triMatrix.TryEmbedCodewords(codewords);

		return triMatrix.GetLowestPenaltyMatrix(errorLevel);
	}
}
