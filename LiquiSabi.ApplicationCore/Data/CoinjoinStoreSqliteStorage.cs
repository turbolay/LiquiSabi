using Microsoft.Data.Sqlite;
using LiquiSabi.ApplicationCore.Utils.Logging;

namespace LiquiSabi.ApplicationCore.Data
{
    public class CoinjoinStoreSqliteStorage : IDisposable
    {
        private bool _disposedValue;
        private readonly SqliteConnection _connection;

        private CoinjoinStoreSqliteStorage(SqliteConnection connection)
        {
            _connection = connection;
        }

        public static CoinjoinStoreSqliteStorage FromFile(string dataSource)
        {
            SqliteConnection? connectionToDispose = null;
            CoinjoinStoreSqliteStorage? storageToDispose = null;

            try
            {
                SqliteConnectionStringBuilder builder = new();
                builder.DataSource = dataSource;
                builder.Pooling = false;

                SqliteConnection connection = new(builder.ConnectionString);
                connectionToDispose = connection;
                connection.Open();

                using (SqliteCommand createCommand = connection.CreateCommand())
                {
                    createCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS coinjoin_rounds (
                            CoordinatorEndpoint TEXT NOT NULL,
                            EstimatedCoordinatorEarningsSats INTEGER NOT NULL,
                            RoundId TEXT NOT NULL PRIMARY KEY,
                            IsBlame INTEGER NOT NULL,
                            CoordinationFeeRate REAL NOT NULL,
                            MinInputCount INTEGER NOT NULL,
                            ParametersMiningFeeRate REAL NOT NULL,
                            RoundStartTime INTEGER NOT NULL,
                            RoundEndTime INTEGER NOT NULL,
                            TxId TEXT NOT NULL,
                            FinalMiningFeeRate REAL NOT NULL,
                            VirtualSize INTEGER NOT NULL,
                            TotalMiningFee INTEGER NOT NULL,
                            InputCount INTEGER NOT NULL,
                            TotalInputAmount INTEGER NOT NULL,
                            FreshInputsEstimateBtc REAL NOT NULL,
                            AverageStandardInputsAnonSet REAL NOT NULL,
                            OutputCount INTEGER NOT NULL,
                            TotalOutputAmount INTEGER NOT NULL,
                            ChangeOutputsAmountRatio REAL NOT NULL,
                            AverageStandardOutputsAnonSet REAL NOT NULL,
                            TotalLeftovers REAL NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_coinjoin_rounds_txid ON coinjoin_rounds(TxId);
                        CREATE INDEX IF NOT EXISTS idx_coinjoin_rounds_endtime ON coinjoin_rounds(RoundEndTime);
                    ";
                    createCommand.ExecuteNonQuery();
                }

                using (SqliteCommand walCommand = connection.CreateCommand())
                {
                    walCommand.CommandText = @"
                        PRAGMA journal_mode = 'wal';
                        PRAGMA synchronous = 'NORMAL';
                    ";
                    walCommand.ExecuteNonQuery();
                }

                CoinjoinStoreSqliteStorage storage = new(connection);
                storageToDispose = storage;
                connectionToDispose = null;

                storageToDispose = null;
                connectionToDispose = null;

                return storage;
            }
            catch (SqliteException ex)
            {
                Logger.LogError($"Failed to open SQLite storage file: {ex.Message}");
                throw;
            }
            finally
            {
                storageToDispose?.Dispose();
                connectionToDispose?.Close();
                connectionToDispose?.Dispose();
            }
        }

        public void Add(CoinjoinStore.SavedRound round)
        {
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO coinjoin_rounds (
                    CoordinatorEndpoint, EstimatedCoordinatorEarningsSats, RoundId, IsBlame,
                    CoordinationFeeRate, MinInputCount, ParametersMiningFeeRate, RoundStartTime,
                    RoundEndTime, TxId, FinalMiningFeeRate, VirtualSize, TotalMiningFee,
                    InputCount, TotalInputAmount, FreshInputsEstimateBtc, AverageStandardInputsAnonSet,
                    OutputCount, TotalOutputAmount, ChangeOutputsAmountRatio, AverageStandardOutputsAnonSet, TotalLeftovers
                ) VALUES (
                    $CoordinatorEndpoint, $EstimatedCoordinatorEarningsSats, $RoundId, $IsBlame,
                    $CoordinationFeeRate, $MinInputCount, $ParametersMiningFeeRate, $RoundStartTime,
                    $RoundEndTime, $TxId, $FinalMiningFeeRate, $VirtualSize, $TotalMiningFee,
                    $InputCount, $TotalInputAmount, $FreshInputsEstimateBtc, $AverageStandardInputsAnonSet,
                    $OutputCount, $TotalOutputAmount, $ChangeOutputsAmountRatio, $AverageStandardOutputsAnonSet, $TotalLeftovers
                )
            ";

            command.Parameters.AddWithValue("$CoordinatorEndpoint", round.CoordinatorEndpoint);
            command.Parameters.AddWithValue("$EstimatedCoordinatorEarningsSats", round.EstimatedCoordinatorEarningsSats);
            command.Parameters.AddWithValue("$RoundId", round.RoundId);
            command.Parameters.AddWithValue("$IsBlame", round.IsBlame ? 1 : 0);
            command.Parameters.AddWithValue("$CoordinationFeeRate", round.CoordinationFeeRate);
            command.Parameters.AddWithValue("$MinInputCount", round.MinInputCount);
            command.Parameters.AddWithValue("$ParametersMiningFeeRate", round.ParametersMiningFeeRate);
            command.Parameters.AddWithValue("$RoundStartTime", round.RoundStartTime.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("$RoundEndTime", round.RoundEndTime.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("$TxId", round.TxId);
            command.Parameters.AddWithValue("$FinalMiningFeeRate", round.FinalMiningFeeRate);
            command.Parameters.AddWithValue("$VirtualSize", round.VirtualSize);
            command.Parameters.AddWithValue("$TotalMiningFee", round.TotalMiningFee);
            command.Parameters.AddWithValue("$InputCount", round.InputCount);
            command.Parameters.AddWithValue("$TotalInputAmount", round.TotalInputAmount);
            command.Parameters.AddWithValue("$FreshInputsEstimateBtc", round.FreshInputsEstimateBtc);
            command.Parameters.AddWithValue("$AverageStandardInputsAnonSet", round.AverageStandardInputsAnonSet);
            command.Parameters.AddWithValue("$OutputCount", round.OutputCount);
            command.Parameters.AddWithValue("$TotalOutputAmount", round.TotalOutputAmount);
            command.Parameters.AddWithValue("$ChangeOutputsAmountRatio", round.ChangeOutputsAmountRatio);
            command.Parameters.AddWithValue("$AverageStandardOutputsAnonSet", round.AverageStandardOutputsAnonSet);
            command.Parameters.AddWithValue("$TotalLeftovers", round.TotalLeftovers);

            command.ExecuteNonQuery();
        }

        public IEnumerable<CoinjoinStore.SavedRound> Get(DateTimeOffset? since = null, DateTimeOffset? until = null, string? coordinatorEndpoint = null)
        {
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM coinjoin_rounds
                WHERE ($Since IS NULL OR RoundEndTime >= $Since)
                AND ($Until IS NULL OR RoundEndTime <= $Until)
                AND ($CoordinatorEndpoint IS NULL OR CoordinatorEndpoint = $CoordinatorEndpoint)
                ORDER BY RoundEndTime
            ";

            command.Parameters.AddWithValue("$Since", since?.ToUnixTimeSeconds() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$Until", until?.ToUnixTimeSeconds() ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$CoordinatorEndpoint", coordinatorEndpoint ?? (object)DBNull.Value);

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                yield return ReadRow(reader);
            }
        }

        public bool IsTxIdKnown(string txId)
        {
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM coinjoin_rounds WHERE TxId = $TxId";
            command.Parameters.AddWithValue("$TxId", txId);

            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        private CoinjoinStore.SavedRound ReadRow(SqliteDataReader reader)
        {
            return new CoinjoinStore.SavedRound(
                CoordinatorEndpoint: reader.GetString(0),
                EstimatedCoordinatorEarningsSats: reader.GetInt64(1),
                RoundId: reader.GetString(2),
                IsBlame: reader.GetBoolean(3),
                CoordinationFeeRate: reader.GetDecimal(4),
                MinInputCount: reader.GetInt32(5),
                ParametersMiningFeeRate: reader.GetDecimal(6),
                RoundStartTime: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(7)),
                RoundEndTime: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(8)),
                TxId: reader.GetString(9),
                FinalMiningFeeRate: reader.GetDecimal(10),
                VirtualSize: reader.GetInt32(11),
                TotalMiningFee: reader.GetInt64(12),
                InputCount: reader.GetInt32(13),
                TotalInputAmount: reader.GetInt64(14),
                FreshInputsEstimateBtc: reader.GetDecimal(15),
                AverageStandardInputsAnonSet: reader.GetDouble(16),
                OutputCount: reader.GetInt32(17),
                TotalOutputAmount: reader.GetInt64(18),
                ChangeOutputsAmountRatio: reader.GetDouble(19),
                AverageStandardOutputsAnonSet: reader.GetDouble(20),
                TotalLeftovers: reader.GetInt64(21)
            );
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _connection.Close();
                    _connection.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}