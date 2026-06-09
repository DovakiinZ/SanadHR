import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function RequestCategoriesPage() {
  return (
    <SimpleMasterDataList
      objectType="RequestCategory"
      title="فئات الطلبات"
      description="تصنيف أنواع الطلبات لتنظيمها في بوابة الموظف"
      backHref="/settings/requests"
      backLabel="إعدادات الطلبات"
    />
  );
}
