import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function AdditionsPage() {
  return (
    <SimpleMasterDataList
      objectType="AdditionType"
      title="الإضافات"
      description="أنواع الإضافات (مكافآت، عمولات، عمل إضافي، حوافز)"
      backHref="/settings/payroll"
      backLabel="إعدادات الرواتب"
    />
  );
}
