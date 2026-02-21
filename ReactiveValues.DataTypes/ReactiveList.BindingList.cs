using System.Collections.Concurrent;
using System.ComponentModel;

namespace ReactiveValues.DataTypes;

partial class ReactiveList<T> : IBindingList
{
	private readonly ConcurrentDictionary<ListChangedEventHandler, (Watcher watcher, Effect effect)>
		listChanged = new();

	bool IBindingList.AllowNew => false;

	bool IBindingList.AllowEdit => false;

	bool IBindingList.AllowRemove => false;

	bool IBindingList.SupportsChangeNotification => true;

	bool IBindingList.SupportsSearching => false;

	bool IBindingList.SupportsSorting => false;

	bool IBindingList.IsSorted => false;

	PropertyDescriptor? IBindingList.SortProperty => throw new NotSupportedException();

	ListSortDirection IBindingList.SortDirection => throw new NotSupportedException();

	event ListChangedEventHandler? IBindingList.ListChanged
	{
		add
		{
			if (value is null) { return; }

			_ = listChanged.AddOrUpdate(
				value,
				value =>
				{
					var watcher = EventHandlerWatcher.Current;

					var args = new ListChangedEventArgs(ListChangedType.Reset, -1);

					var effect = Reactive.EventEffect(count, () => value(this, args));

					watcher.Watch(effect);

					return (watcher, effect);
				},
				(value, _) => throw new InvalidOperationException()
				);
		}

		remove
		{
			if (value is null) { return; }

			if (listChanged.TryRemove(value, out var result) is false)
			{
				throw new InvalidOperationException();
			}

			result.watcher.Unwatch(result.effect);
		}
	}

	void IBindingList.AddIndex(PropertyDescriptor property) => throw new NotSupportedException();

	object IBindingList.AddNew() => throw new NotSupportedException();

	void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) => throw new NotSupportedException();

	int IBindingList.Find(PropertyDescriptor property, object key) => throw new NotSupportedException();

	void IBindingList.RemoveIndex(PropertyDescriptor property) => throw new NotSupportedException();

	void IBindingList.RemoveSort() => throw new NotSupportedException();
}
