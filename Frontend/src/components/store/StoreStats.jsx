import { FiBriefcase, FiMap, FiMapPin, FiUsers } from 'react-icons/fi';

function buildStats(stores = []) {
  const cityCount = new Set(stores.map((store) => store.city).filter(Boolean)).size;

  return [
    {
      label: 'Cua hang',
      value: String(stores.length),
      Icon: FiMapPin,
    },
    {
      label: 'Tinh thanh',
      value: String(cityCount),
      Icon: FiMap,
    },
    {
      label: 'Van phong dai dien',
      value: '3',
      Icon: FiBriefcase,
    },
    {
      label: 'Nhan su',
      value: '500+',
      Icon: FiUsers,
    },
  ];
}

function StoreStats({ stores = [] }) {
  const stats = buildStats(stores);

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {stats.map((item) => {
        const Icon = item.Icon;

        return (
          <div
            key={item.label}
            className="flex items-center gap-4 rounded-2xl border border-zinc-100 bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.06)]"
          >
            <span className="grid h-14 w-14 shrink-0 place-items-center rounded-2xl bg-[#fff4f4]">
              <Icon className="h-7 w-7 text-[#d71920]" aria-hidden="true" />
            </span>
            <span className="text-[15px] font-bold text-zinc-700">
              {item.label}
              <strong className="mt-1 block text-[28px] leading-none font-black text-[#d71920]">{item.value}</strong>
            </span>
          </div>
        );
      })}
    </div>
  );
}

export default StoreStats;
