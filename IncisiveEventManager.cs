using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using System;

namespace CTHarmonyAdapters
{
    internal class IncisiveEventManager : DataModificationEventsBase
    {
        public override bool IsEventSubscriptionSupported(string deviceId)
        {
            return false;
        }

        public override void SubscribeToBlobAddedEvent(StorageKey referenceStorageKey, EventHandler<BlobAddedEventArgs> blobAdded)
        {
        }

        public override void SubscribeToBlobDeletedEvent(StorageKey referenceStorageKey, EventHandler<BlobDeletedEventArgs> blobDeleted)
        {
        }

        public override void SubscribeToImageAddedEvent(StorageKey parentStorageKey, EventHandler<ImageAddedEventArgs> imageAdded)
        {
        }

        public override void SubscribeToImageAddedEvent(string deviceId, EventHandler<ImageAddedEventArgs> imageAdded)
        {
        }

        public override void SubscribeToImageDeletedEvent(StorageKey parentStorageKey, EventHandler<ImageDeletedEventArgs> imageDeleted)
        {
        }

        public override void SubscribeToImageDeletedEvent(string deviceId, EventHandler<ImageDeletedEventArgs> imageDeleted)
        {
        }

        public override void SubscribeToMFImageAddedEvent(StorageKey studyKey, EventHandler<MFImageAddedEventArgs> mfImageAdded)
        {
        }

        public override void SubscribeToMFImageAddedEvent(string deviceId, EventHandler<MFImageAddedEventArgs> mfImageAdded)
        {
        }

        public override void SubscribeToMFImageDeletedEvent(StorageKey studyKey, EventHandler<MFImageDeletedEventArgs> mfImageDeleted)
        {
        }

        public override void SubscribeToSeriesAddedEvent(StorageKey parentStudyKey, EventHandler<SeriesAddedEventArgs> seriesAdded)
        {

        }

        public override void SubscribeToSeriesAddedEvent(string deviceId, EventHandler<SeriesAddedEventArgs> seriesAddedHandler)
        {

        }

        public override void SubscribeToSeriesCompletedEvent(StorageKey parentStudyKey, EventHandler<SeriesCompletedEventArgs> seriesCompleted)
        {

        }

        public override void SubscribeToSeriesCompletedEvent(string deviceId, EventHandler<SeriesCompletedEventArgs> seriesCompleted)
        {

        }

        public override void SubscribeToSeriesDeletedEvent(StorageKey studyKey, EventHandler<SeriesDeletedEventArgs> seriesDeleted)
        {

        }

        public override void SubscribeToSeriesDeletedEvent(string deviceId, EventHandler<SeriesDeletedEventArgs> seriesDeletedHandler)
        {

        }

        public override void SubscribeToSeriesModifiedEvent(StorageKey studyKey, EventHandler<SeriesModifiedEventArgs> seriesModified)
        {

        }

        public override void SubscribeToStudyAddedEvent(string deviceId, EventHandler<StudyAddedEventArgs> studyAddedHandler)
        {

        }

        public override void SubscribeToStudyAttributesModifiedEvent(StorageKey studyKey, EventHandler<StudyModifiedEventArgs> studyModified)
        {

        }

        public override void SubscribeToStudyAttributesModifiedEvent(string deviceId, EventHandler<StudyModifiedEventArgs> onStudyModified)
        {

        }

        public override void SubscribeToStudyCompletedEvent(string deviceId, EventHandler<StudyCompletedEventArgs> studyCompletedHandler)
        {

        }

        public override void SubscribeToStudyDeletedEvent(string deviceId, EventHandler<StudyDeletedEventArgs> studyDeletedHandler)
        {

        }

        public override void SubscribeToStudyUpdatedEvent(string deviceId, EventHandler<StudyUpdatedEventArgs> studyUpdateHandler)
        {

        }

        public override void UnsubscribeFromBlobAddedEvent(StorageKey referenceStorageKey, EventHandler<BlobAddedEventArgs> blobAdded)
        {

        }

        public override void UnsubscribeFromBlobDeletedEvent(StorageKey referenceStorageKey, EventHandler<BlobDeletedEventArgs> blobDeleted)
        {

        }

        public override void UnsubscribeFromImageAddedEvent(StorageKey parentStorageKey, EventHandler<ImageAddedEventArgs> imageAdded)
        {

        }

        public override void UnsubscribeFromImageAddedEvent(string deviceId, EventHandler<ImageAddedEventArgs> imageAdded)
        {

        }

        public override void UnsubscribeFromImageDeletedEvent(StorageKey parentStorageKey, EventHandler<ImageDeletedEventArgs> imageDeleted)
        {

        }

        public override void UnsubscribeFromImageDeletedEvent(string deviceId, EventHandler<ImageDeletedEventArgs> imageDeleted)
        {

        }

        public override void UnsubscribeFromMFImageAddedEvent(StorageKey studyKey, EventHandler<MFImageAddedEventArgs> mfImageAdded)
        {

        }

        public override void UnsubscribeFromMFImageAddedEvent(string deviceId, EventHandler<MFImageAddedEventArgs> mfImageAdded)
        {

        }

        public override void UnsubscribeFromMFImageDeletedEvent(StorageKey studyKey, EventHandler<MFImageDeletedEventArgs> mfImageDeleted)
        {

        }

        public override void UnsubscribeFromSeriesAddedEvent(StorageKey studyKey, EventHandler<SeriesAddedEventArgs> seriesAdded)
        {

        }

        public override void UnsubscribeFromSeriesAddedEvent(string deviceId, EventHandler<SeriesAddedEventArgs> seriesAdded)
        {

        }

        public override void UnsubscribeFromSeriesCompletedEvent(string deviceId, EventHandler<SeriesCompletedEventArgs> seriesCompleted)
        {

        }

        public override void UnsubscribeFromSeriesCompletedEvent(StorageKey parentStudyKey, EventHandler<SeriesCompletedEventArgs> seriesCompleted)
        {

        }

        public override void UnsubscribeFromSeriesDeletedEvent(StorageKey studyKey, EventHandler<SeriesDeletedEventArgs> seriesDeleted)
        {

        }

        public override void UnsubscribeFromSeriesDeletedEvent(string deviceId, EventHandler<SeriesDeletedEventArgs> seriesDeleted)
        {

        }

        public override void UnsubscribeFromSeriesModifiedEvent(StorageKey studyKey, EventHandler<SeriesModifiedEventArgs> seriesModified)
        {

        }

        public override void UnsubscribeFromStudyAddedEvent(string deviceId, EventHandler<StudyAddedEventArgs> studyModified)
        {

        }

        public override void UnsubscribeFromStudyAttributesModifiedEvent(StorageKey studyKey, EventHandler<StudyModifiedEventArgs> studyModified)
        {

        }

        public override void UnsubscribeFromStudyAttributesModifiedEvent(string deviceId, EventHandler<StudyModifiedEventArgs> onStudyModified)
        {

        }

        public override void UnsubscribeFromStudyCompletedEvent(string deviceId, EventHandler<StudyCompletedEventArgs> studyModified)
        {

        }

        public override void UnsubscribeFromStudyDeletedEvent(string deviceId, EventHandler<StudyDeletedEventArgs> studyModified)
        {

        }

        public override void UnsubscribeFromStudyUpdatedEvent(string deviceId, EventHandler<StudyUpdatedEventArgs> studyUpdate)
        {

        }
    }
}
