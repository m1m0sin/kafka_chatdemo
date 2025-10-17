using System;

namespace chatApp.Services;

public interface IKafkaTopicInitializer
{
    Task EnsureTopicsExistAsync();
}
